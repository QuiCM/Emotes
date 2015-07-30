
using System;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.GameContent.UI;

using TerrariaApi.Server;
using TShockAPI;

namespace Emotes
{
	[ApiVersion(1, 20)]
	public class Plugin : TerrariaPlugin
	{
		internal Regex ChatRegex;
		internal Regex SmileyRegex;
		internal Random r;

		public override string Author
		{
			get
			{
				return "White";
			}
		}

		public override string Description
		{
			get
			{
				return "Displays Terraria emotes when specific text is sent";
			}
		}

		public override string Name
		{
			get
			{
				return "Emotes";
			}
		}

		public override Version Version
		{
			get
			{
				return new Version(1, 0);
			}
		}

		public Plugin(Main game) : base(game)
		{
		}

		public override void Initialize()
		{
			ChatRegex = new Regex(@"\:(\S+)\:");
			SmileyRegex = new Regex(@"(?<happy>[:;=][,.'^o-]?[)D])|(?<heart><3)|(?<kissy>[:;=][,.'^o-]?\*)|(?<sleepy>[zZ]{3,5})|(?<confuse>\?{2,5})|(?<angry>[D>\]][:;=][,.'^0-]?[\(<\/])|(?<cry>(?>[:=][,'][\(\/])|(?>[T;][-_][T;]))");
			r = new Random();

			ServerApi.Hooks.ServerChat.Register(this, OnChat, 6);
		}

		private void OnChat(ServerChatEventArgs args)
		{
			if (!ChatRegex.IsMatch(args.Text) && !SmileyRegex.IsMatch(args.Text))
			{
				return;
			}

			if (SmileyRegex.IsMatch(args.Text))
			{
				SendSmiley(SmileyRegex.Match(args.Text), args.Who);
			}
			else
			{
				SendOtherEmote(ref args);
			}
		}

		private void SendSmiley(Match match, int who)
		{
			int ID = EmoteBubble.AssignNewID();
			int emoteID;

			if (!String.IsNullOrEmpty(match.Groups["happy"].Value))
			{
				emoteID = EmoteID.EmoteLaugh;
			}
			else if (!String.IsNullOrEmpty(match.Groups["heart"].Value))
			{
				emoteID = EmoteID.EmotionLove;
			}
			else if (!String.IsNullOrEmpty(match.Groups["kissy"].Value))
			{
				emoteID = EmoteID.EmoteKiss;
			}
			else if (!String.IsNullOrEmpty(match.Groups["sleepy"].Value))
			{
				emoteID = EmoteID.EmoteSleep;
			}
			else if (!String.IsNullOrEmpty(match.Groups["confuse"].Value))
			{
				emoteID = EmoteID.EmoteConfused;
			}
			else if (!String.IsNullOrEmpty(match.Groups["angry"].Value))
			{
				emoteID = EmoteID.EmotionAnger;
			}
			else if (!String.IsNullOrEmpty(match.Groups["cry"].Value))
			{
				emoteID = EmoteID.EmotionCry;
			}
			else
			{
				return;
			}

			NetMessage.SendData(91, -1, -1, "", ID, 1, who, 600, emoteID);
		}

		private void SendOtherEmote(ref ServerChatEventArgs args)
		{
			int ID = EmoteBubble.AssignNewID();
			int emoteID;
			
			string match = ChatRegex.Match(args.Text).Groups[1].Value;
			
			switch (match.ToLower())
			{
				case "happy":
				case "smile":
				case "laugh":
				case "lol":
					emoteID = EmoteID.EmoteLaugh;
					break;

				case "rock-paper-scissors":
				case "rps":
				case "r-p-s":
					emoteID = r.Next(EmoteID.RPSScissors, EmoteID.RPSPaper + 1);
					break;

				case "love":
				case "<3":
					emoteID = EmoteID.EmotionLove;
					break;

				default:
					return;
			}

			NetMessage.SendData(91, -1, -1, "", ID, 1, args.Who, 600, emoteID);
			args.Handled = true;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				ServerApi.Hooks.ServerChat.Deregister(this, OnChat);
			}
			base.Dispose(disposing);
		}
	}
}
