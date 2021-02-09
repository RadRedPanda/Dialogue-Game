using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts
{
	public enum Expression
	{
		Normal,
		Happy,
		Sad,
		Confused,
		Angry
	}

	public enum DialogueSpeed
	{
		Normal,
		Slow,
		Fast
	}

	public enum Keyword
	{
		yes,
		no
	}

	[Serializable]
	public class DialogueClass
	{
		public Expression Expression; // art state we should be setting to the speaker
		public DialogueSpeed Speed; // how fast the text is displayed
		public List<string> Dialogue; // list that contains all the dialogue for the node
		public Dictionary<Keyword, string> Options; // dictionary of option -> guids that this one connects to

		//
		// Summary:
		//     Turns the string input into the Keyword enum
		//
		// Parameters:
		//   input:
		//     String or object to be converted to Keyword enum for use.
		public static Keyword ParseStringToKeyword(string input)
		{
			try // the Parse method needs a try/catch block, catches ArgumentException if the word isn't in the master list of keywords
			{
				Keyword keywordValue = (Keyword)Enum.Parse(typeof(Keyword), input, true);
				return keywordValue;
			}
			catch (ArgumentException)
			{
				Console.WriteLine("{0} is not a member of the Keyword enumeration.", input);
				return (Keyword)0;
			}
		}

		public DialogueClass(Expression e, DialogueSpeed s, List<string> d, Dictionary<Keyword, string> o)
		{
			Expression = e;
			Speed = s;
			Dialogue = d;
			Options = o;
		}

		public DialogueClass(DialogueNodeData nodeData, List<NodeLinkData> nodeLinks)
		{
			Dialogue = nodeData.DialogueText.Split('\n').ToList();
			Options = new Dictionary<Keyword, string>();
			foreach (NodeLinkData nodeLink in nodeLinks)
				Options.Add(ParseStringToKeyword(nodeLink.PortName), nodeLink.TargetNodeGuid);
			Expression = nodeData.Expression;
			Speed = nodeData.Speed;
		}
	}
}