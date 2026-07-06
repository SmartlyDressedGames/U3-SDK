////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;


namespace SDG.Unturned
{
	public class MenuCreditsUI
	{
		private static SleekFullscreenBox container;
		public static bool active;

		private static SleekButtonIcon returnButton;
		private static ISleekBox creditsBox;
		private static ISleekScrollView scrollBox;

		public static void open()
		{
			if (active)
			{
				return;
			}

			active = true;

			container.AnimateIntoView();
		}

		public static void close()
		{
			if (!active)
			{
				return;
			}

			active = false;

			container.AnimateOutOfView(0, -1);
		}

		private static void onClickedReturnButton(ISleekElement button)
		{
			close();
			MenuPauseUI.open();
		}

		public MenuCreditsUI()
		{
			localization = Localization.read("/Menu/MenuCredits.dat");

			container = new SleekFullscreenBox();
			container.PositionOffset_X = 10;
			container.PositionOffset_Y = 10;
			container.PositionScale_Y = -1;
			container.SizeOffset_X = -20;
			container.SizeOffset_Y = -20;
			container.SizeScale_X = 1;
			container.SizeScale_Y = 1;
			MenuUI.container.AddChild(container);
			active = false;

			returnButton = new SleekButtonIcon(MenuPauseUI.icons.load<Texture2D>("Exit"));
			returnButton.PositionOffset_X = -250;
			returnButton.PositionOffset_Y = 100;
			returnButton.PositionScale_X = 0.5f;
			returnButton.SizeOffset_X = 500;
			returnButton.SizeOffset_Y = 50;
			returnButton.text = MenuPauseUI.localization.format("Return_Button");
			returnButton.tooltip = MenuPauseUI.localization.format("Return_Button_Tooltip");
			returnButton.onClickedButton += onClickedReturnButton;
			returnButton.fontSize = ESleekFontSize.Medium;
			returnButton.iconColor = ESleekTint.FOREGROUND;
			container.AddChild(returnButton);

			creditsBox = Glazier.Get().CreateBox();
			creditsBox.PositionOffset_X = -250;
			creditsBox.PositionOffset_Y = 160;
			creditsBox.PositionScale_X = 0.5f;
			creditsBox.SizeOffset_X = 500;
			creditsBox.SizeOffset_Y = -260;
			creditsBox.SizeScale_Y = 1;
			creditsBox.FontSize = ESleekFontSize.Medium;
			container.AddChild(creditsBox);

			scrollBox = Glazier.Get().CreateScrollView();
			scrollBox.PositionOffset_X = 5;
			scrollBox.PositionOffset_Y = 5;
			scrollBox.SizeOffset_X = -10;
			scrollBox.SizeOffset_Y = -10;
			scrollBox.SizeScale_X = 1;
			scrollBox.SizeScale_Y = 1;
			scrollBox.ScaleContentToWidth = true;
			creditsBox.AddChild(scrollBox);

			float verticalOffset = 0;
			AddHeader(localization.format("Header_Unturned"), ref verticalOffset);

			AddRow("Nelson Sexton", "Developer", ref verticalOffset);
			AddRow("Tyler \"MoltonMontro\" Pope", "Community Manager", ref verticalOffset);
			AddRow("Dogfood", "Additional Content (2025-2026)", ref verticalOffset);
			AddRow("Sven Mawby", "RocketMod", ref verticalOffset);
			AddRow("Riley Labrecque", "Steamworks .NET", ref verticalOffset);
			AddRow("Stephen McKamey", "A* Pathfinding Project", ref verticalOffset);
			AddRow("James Newton-King", "Json .NET", ref verticalOffset);
			AddRow("Still North Media", "The Firearm Sound Library", ref verticalOffset);
			AddRow("Peter Wayne", "GameMaster Audio Pro Sound Collection", ref verticalOffset);
			AddRow("John '00' Fleming", "Title Music", ref verticalOffset);
			AddRow("staswalle", "Loading Screen Music", ref verticalOffset);

			AddHeader(localization.format("Header_CommunityTeam"), ref verticalOffset);

			string[] communityTeam = new string[]
			{
				"Deathismad",
				"James",
				"Retuuyo",
				"Fran-war",
				"SongPhoenix",
				"Lu",
				"Morkva",
				"Reaver",
				"Shadow",
				"Yarrrr",
				"DeusExMachina", // Discord
				"Pablo824", // Discord
				"Genestic12", // Discord
				"Armaros", // SDG Forum
				"Great Hero J", // SDG Forum
				"SomeCatIDK", // Discord
				"Jdance", // Discord
				"cucuycharles", // Discord
			};
			System.Array.Sort(communityTeam);
			AddRowColumns(communityTeam, ref verticalOffset);

			AddHeader(localization.format("Header_MapCreators"), ref verticalOffset);

			string[] mapCreators = new string[]
			{
				"Nicolas \"Putin3D\" Arisi",
				"Mia \"Myria\" Brookman",
				"Ben \"Paladin\" Hoefer",
				"Nathan \"Wolf_Maniac\" Zwerka",
				"Nolan \"Nolamo\" Ross",
				"Husky",
				"Emily Barry",
				"Justin \"Gamez2much\" Morton",
				"Terran \"Spyjack\" Orion",
				"Alex \"Rain\" Storanov",
				"Amanda \"Mooki2much\" Hubler",
				"Joshua \"Storm_Epidemic\" Rist",
				"Th3o",
				"Diesel_Sisel",
				"Misterl212",
				"Mitch \"Sketches\" Wheaton",
				"AnimaticFreak",
				"NSTM",
				"Maciej \"Renaxon\" Maziarz",
				"Daniel \"danaby2\" Segboer",
				"Dug",
				"Thom \"Spebby\" Mott",
				"Steven \"MeloCa\" Nadeau",
				"Ethan \"Vilespring\" Lossner",
				"SluggedCascade",
				"Sam \"paper_walls84\" Clerke",
				"clue",
				"Vilaskis \"BATTLEKOT\" Shaleshev",
				"Andrii \"TheCubicNoobik\" Vitiv",
				"Oleksandr \"BlackLion\" Shcherba",
				"Dmitriy \"Potatoes\" Usenko",
				"Liya \"Ms.Evrika\" Bognat",
				"Denis \"Flodo\" Souza",
				"João \"L2\" Vitor",
				"Josh \"Leprechan12\" Hogan",
				"Toothy Deerryte",
				"Witness Protection",
				"Maria \"Zefirka\" Kosyakova",
				"Sultan \"Sultan\" Sultanović",
				"LVOmega",
				"janeks",
				"Ivan \"August\" Hrynkevych",
			};
			System.Array.Sort(mapCreators);
			AddRowColumns(mapCreators, ref verticalOffset);

			scrollBox.ContentSizeOffset = new Vector2(0.0f, verticalOffset);
		}

		private static void AddHeader(string key, ref float verticalOffset)
		{
			ISleekLabel label = Glazier.Get().CreateLabel();
			label.PositionOffset_Y = verticalOffset;
			label.SizeOffset_Y = 50;
			label.SizeScale_X = 1.0f;
			label.TextAlignment = TextAnchor.MiddleCenter;
			label.FontSize = ESleekFontSize.Large;
			label.Text = localization.format(key);
			scrollBox.AddChild(label);
			verticalOffset += label.SizeOffset_Y;
		}

		private static void AddRow(string contributor, string contribution, ref float verticalOffset)
		{
			ISleekLabel contributorLabel = Glazier.Get().CreateLabel();
			contributorLabel.PositionOffset_Y = verticalOffset;
			contributorLabel.SizeOffset_Y = 30;
			contributorLabel.SizeScale_X = 1;
			contributorLabel.TextAlignment = TextAnchor.MiddleLeft;
			contributorLabel.FontSize = ESleekFontSize.Medium;
			contributorLabel.Text = contributor;
			scrollBox.AddChild(contributorLabel);

			ISleekLabel contributionLabel = Glazier.Get().CreateLabel();
			contributionLabel.PositionOffset_Y = verticalOffset;
			contributionLabel.SizeOffset_Y = 30;
			contributionLabel.SizeScale_X = 1;
			contributionLabel.TextAlignment = TextAnchor.MiddleRight;
			contributionLabel.FontSize = ESleekFontSize.Medium;
			contributionLabel.Text = contribution;
			scrollBox.AddChild(contributionLabel);

			verticalOffset += 30;
		}

		private static void AddRowColumns(string[] contributors, ref float verticalOffset)
		{
			int column = 0;
			foreach (string contributor in contributors)
			{
				ISleekLabel label = Glazier.Get().CreateLabel();
				label.PositionOffset_Y = verticalOffset;
				label.PositionScale_X = column * 0.5f;
				label.SizeOffset_Y = 30;
				label.SizeScale_X = 0.5f;
				label.TextAlignment = TextAnchor.MiddleCenter;
				label.FontSize = ESleekFontSize.Medium;
				label.Text = contributor;
				scrollBox.AddChild(label);

				++column;
				if (column >= 2)
				{
					column = 0;
					verticalOffset += 30;
				}
			}

			if (column > 0)
			{
				verticalOffset += 30;
			}
		}

		private static Local localization;
	}
}
