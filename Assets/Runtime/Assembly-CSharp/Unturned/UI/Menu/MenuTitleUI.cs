////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the U3 SDK: https://github.com/smartlydressedgames/u3-sdk/    //
// Please refer to the included LICENSE.txt for copyright notice and license details. //
////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace SDG.Unturned
{
	public class MenuTitleUI
	{
		private static readonly byte STAT_COUNT = 18;

		private static Local localization;
		private static SleekFullscreenBox container;
		public static bool active;

		private static ISleekBox titleBox;
		private static ISleekLabel titleLabel;
		private static ISleekLabel authorLabel;

		//private static SleekButton proButton;
		//private static SleekButton infoButton;
		//private static ISleekLabel proLabel;
		//private static ISleekLabel featureLabel;
		//private static ISleekLabel infoLabel;
		//private static ISleekLabel newsLabel;

		private static ISleekButton statButton;
		private static EPlayerStat stat;

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

			container.AnimateOutOfView(0, 1);
		}

		private static void onClickedStatButton(ISleekElement button)
		{
			byte newStat;
			do
			{
				newStat = (byte) Random.Range(1, STAT_COUNT + 1);
			}
			while (newStat == (byte) stat);
			stat = (EPlayerStat) newStat;

			if (stat == EPlayerStat.KILLS_ZOMBIES_NORMAL)
			{
				int data;
				Provider.provider.statisticsService.userStatisticsService.getStatistic("Kills_Zombies_Normal", out data);

				long global;
				Provider.provider.statisticsService.globalStatisticsService.getStatistic("Kills_Zombies_Normal", out global);

				statButton.Text = localization.format("Stat_Kills_Zombies_Normal", data.ToString("n0"), global.ToString("n0"));
			}
			else if (stat == EPlayerStat.KILLS_PLAYERS)
			{
				int data;
				Provider.provider.statisticsService.userStatisticsService.getStatistic("Kills_Players", out data);

				long global;
				Provider.provider.statisticsService.globalStatisticsService.getStatistic("Kills_Players", out global);

				statButton.Text = localization.format("Stat_Kills_Players", data.ToString("n0"), global.ToString("n0"));
			}
			else if (stat == EPlayerStat.FOUND_ITEMS)
			{
				int data;
				Provider.provider.statisticsService.userStatisticsService.getStatistic("Found_Items", out data);

				long global;
				Provider.provider.statisticsService.globalStatisticsService.getStatistic("Found_Items", out global);

				statButton.Text = localization.format("Stat_Found_Items", data.ToString("n0"), global.ToString("n0"));
			}
			else if (stat == EPlayerStat.FOUND_RESOURCES)
			{
				int data;
				Provider.provider.statisticsService.userStatisticsService.getStatistic("Found_Resources", out data);

				long global;
				Provider.provider.statisticsService.globalStatisticsService.getStatistic("Found_Resources", out global);

				statButton.Text = localization.format("Stat_Found_Resources", data.ToString("n0"), global.ToString("n0"));
			}
			else if (stat == EPlayerStat.FOUND_EXPERIENCE)
			{
				int data;
				Provider.provider.statisticsService.userStatisticsService.getStatistic("Found_Experience", out data);

				long global;
				Provider.provider.statisticsService.globalStatisticsService.getStatistic("Found_Experience", out global);

				statButton.Text = localization.format("Stat_Found_Experience", data.ToString("n0"), global.ToString("n0"));
			}
			else if (stat == EPlayerStat.KILLS_ZOMBIES_MEGA)
			{
				int data;
				Provider.provider.statisticsService.userStatisticsService.getStatistic("Kills_Zombies_Mega", out data);

				long global;
				Provider.provider.statisticsService.globalStatisticsService.getStatistic("Kills_Zombies_Mega", out global);

				statButton.Text = localization.format("Stat_Kills_Zombies_Mega", data.ToString("n0"), global.ToString("n0"));
			}
			else if (stat == EPlayerStat.DEATHS_PLAYERS)
			{
				int data;
				Provider.provider.statisticsService.userStatisticsService.getStatistic("Deaths_Players", out data);

				long global;
				Provider.provider.statisticsService.globalStatisticsService.getStatistic("Deaths_Players", out global);

				statButton.Text = localization.format("Stat_Deaths_Players", data.ToString("n0"), global.ToString("n0"));
			}
			else if (stat == EPlayerStat.KILLS_ANIMALS)
			{
				int data;
				Provider.provider.statisticsService.userStatisticsService.getStatistic("Kills_Animals", out data);

				long global;
				Provider.provider.statisticsService.globalStatisticsService.getStatistic("Kills_Animals", out global);

				statButton.Text = localization.format("Stat_Kills_Animals", data.ToString("n0"), global.ToString("n0"));
			}
			else if (stat == EPlayerStat.FOUND_CRAFTS)
			{
				int data;
				Provider.provider.statisticsService.userStatisticsService.getStatistic("Found_Crafts", out data);

				long global;
				Provider.provider.statisticsService.globalStatisticsService.getStatistic("Found_Crafts", out global);

				statButton.Text = localization.format("Stat_Found_Crafts", data.ToString("n0"), global.ToString("n0"));
			}
			else if (stat == EPlayerStat.FOUND_FISHES)
			{
				int data;
				Provider.provider.statisticsService.userStatisticsService.getStatistic("Found_Fishes", out data);

				long global;
				Provider.provider.statisticsService.globalStatisticsService.getStatistic("Found_Fishes", out global);

				statButton.Text = localization.format("Stat_Found_Fishes", data.ToString("n0"), global.ToString("n0"));
			}
			else if (stat == EPlayerStat.FOUND_PLANTS)
			{
				int data;
				Provider.provider.statisticsService.userStatisticsService.getStatistic("Found_Plants", out data);

				long global;
				Provider.provider.statisticsService.globalStatisticsService.getStatistic("Found_Plants", out global);

				statButton.Text = localization.format("Stat_Found_Plants", data.ToString("n0"), global.ToString("n0"));
			}
			else if (stat == EPlayerStat.ACCURACY)
			{
				int dataShot;
				Provider.provider.statisticsService.userStatisticsService.getStatistic("Accuracy_Shot", out dataShot);

				int dataHit;
				Provider.provider.statisticsService.userStatisticsService.getStatistic("Accuracy_Hit", out dataHit);

				long globalShot;
				Provider.provider.statisticsService.globalStatisticsService.getStatistic("Accuracy_Shot", out globalShot);

				long globalHit;
				Provider.provider.statisticsService.globalStatisticsService.getStatistic("Accuracy_Hit", out globalHit);

				float data;
				if (dataShot == 0 || dataHit == 0)
				{
					data = 0;
				}
				else
				{
					data = dataHit / (float) dataShot;
				}

				double global;
				if (globalShot == 0 || globalHit == 0)
				{
					global = 0;
				}
				else
				{
					global = globalHit / (double) globalShot;
				}

				statButton.Text = localization.format("Stat_Accuracy", dataShot.ToString("n0"), (int) (data * 10000) / 100.0f, globalShot.ToString("n0"), (long) (global * 10000) / 100.0);
			}
			else if (stat == EPlayerStat.HEADSHOTS)
			{
				int data;
				Provider.provider.statisticsService.userStatisticsService.getStatistic("Headshots", out data);

				long global;
				Provider.provider.statisticsService.globalStatisticsService.getStatistic("Headshots", out global);

				statButton.Text = localization.format("Stat_Headshots", data.ToString("n0"), global.ToString("n0"));
			}
			else if (stat == EPlayerStat.TRAVEL_FOOT)
			{
				int data;
				Provider.provider.statisticsService.userStatisticsService.getStatistic("Travel_Foot", out data);

				long global;
				Provider.provider.statisticsService.globalStatisticsService.getStatistic("Travel_Foot", out global);

				if (OptionsSettings.metric)
				{
					statButton.Text = localization.format("Stat_Travel_Foot", data.ToString("n0") + " m", global.ToString("n0") + " m");
				}
				else
				{
					statButton.Text = localization.format("Stat_Travel_Foot", MeasurementTool.MtoYd(data).ToString("n0") + " yd", MeasurementTool.MtoYd(global).ToString("n0") + " yd");
				}
			}
			else if (stat == EPlayerStat.TRAVEL_VEHICLE)
			{
				int data;
				Provider.provider.statisticsService.userStatisticsService.getStatistic("Travel_Vehicle", out data);

				long global;
				Provider.provider.statisticsService.globalStatisticsService.getStatistic("Travel_Vehicle", out global);

				if (OptionsSettings.metric)
				{
					statButton.Text = localization.format("Stat_Travel_Vehicle", data.ToString("n0") + " m", global.ToString("n0") + " m");
				}
				else
				{
					statButton.Text = localization.format("Stat_Travel_Vehicle", MeasurementTool.MtoYd(data).ToString("n0") + " yd", MeasurementTool.MtoYd(global).ToString("n0") + " yd");
				}
			}
			else if (stat == EPlayerStat.ARENA_WINS)
			{
				int data;
				Provider.provider.statisticsService.userStatisticsService.getStatistic("Arena_Wins", out data);

				long global;
				Provider.provider.statisticsService.globalStatisticsService.getStatistic("Arena_Wins", out global);

				statButton.Text = localization.format("Stat_Arena_Wins", data.ToString("n0"), global.ToString("n0"));
			}
			else if (stat == EPlayerStat.FOUND_BUILDABLES)
			{
				int data;
				Provider.provider.statisticsService.userStatisticsService.getStatistic("Found_Buildables", out data);

				long global;
				Provider.provider.statisticsService.globalStatisticsService.getStatistic("Found_Buildables", out global);

				statButton.Text = localization.format("Stat_Found_Buildables", data.ToString("n0"), global.ToString("n0"));
			}
			else if (stat == EPlayerStat.FOUND_THROWABLES)
			{
				int data;
				Provider.provider.statisticsService.userStatisticsService.getStatistic("Found_Throwables", out data);

				long global;
				Provider.provider.statisticsService.globalStatisticsService.getStatistic("Found_Throwables", out global);

				statButton.Text = localization.format("Stat_Found_Throwables", data.ToString("n0"), global.ToString("n0"));
			}
		}

		public MenuTitleUI()
		{
			localization = Localization.read("/Menu/MenuTitle.dat");

			container = new SleekFullscreenBox();
			container.PositionOffset_X = 10;
			container.PositionOffset_Y = 10;
			container.SizeOffset_X = -20;
			container.SizeOffset_Y = -20;
			container.SizeScale_X = 1;
			container.SizeScale_Y = 1;
			MenuUI.container.AddChild(container);
			active = true;

			titleBox = Glazier.Get().CreateBox();
			//			titleBox.positionOffset_X = -100;
			//			titleBox.positionOffset_Y = -50;
			//			titleBox.positionScale_X = 0.5f;
			//			titleBox.positionScale_Y = 0.25f;
			//			titleBox.sizeOffset_X = 400;
			titleBox.SizeOffset_Y = 100;
			titleBox.SizeScale_X = 1;
			container.AddChild(titleBox);

			titleLabel = Glazier.Get().CreateLabel();
			titleLabel.SizeScale_X = 1;
			titleLabel.SizeOffset_Y = 70;
			titleLabel.FontSize = ESleekFontSize.Title;
			titleLabel.Text = Provider._modInfo != null ? Provider._modInfo.Name : Provider.APP_NAME;

			titleBox.AddChild(titleLabel);

			authorLabel = Glazier.Get().CreateLabel();
			authorLabel.PositionOffset_Y = 60;
			authorLabel.SizeScale_X = 1;
			authorLabel.SizeOffset_Y = 30;
			if (Provider._modInfo != null)
			{
				authorLabel.Text = localization.format("Author_Label", Provider._modInfo.FormatModVersion(), Provider._modInfo.Creators);
			}
			else
			{
				authorLabel.Text = localization.format("Author_Label", Provider.APP_VERSION, Provider.APP_AUTHOR);
			}
			titleBox.AddChild(authorLabel);

			//infoButton = Glazier.Get().CreateButton();
			////			infoButton.positionOffset_X = -100;
			////			infoButton.positionOffset_Y = -50;
			////			infoButton.positionScale_X = 0.5f;
			////			infoButton.positionScale_Y = 0.5f;
			////			infoButton.sizeOffset_X = 400;
			//infoButton.positionOffset_X = 5;
			//infoButton.positionOffset_Y = -100;
			//infoButton.positionScale_X = 0.5f;
			//infoButton.positionScale_Y = 1;
			//infoButton.sizeOffset_X = -5;
			//infoButton.sizeOffset_Y = 100;
			//infoButton.sizeScale_X = 0.5f;
			//infoButton.tooltip = localization.format("Info_Button_Tooltip");
			//infoButton.onClickedButton += onClickedInfoButton;
			//container.add(infoButton);

			//infoLabel = Glazier.Get().CreateLabel();
			//infoLabel.sizeScale_X = 1;
			//infoLabel.sizeOffset_Y = 50;
			//infoLabel.text = localization.format("Info_Title");
			//infoLabel.fontSize = ESleekFontSize.Large;
			//infoButton.add(infoLabel);

			//newsLabel = Glazier.Get().CreateLabel();
			//newsLabel.positionOffset_Y = 50;
			//newsLabel.sizeOffset_Y = -50;
			//newsLabel.sizeScale_X = 1;
			//newsLabel.sizeScale_Y = 1;
			//newsLabel.text = localization.format("Info_Button");
			//infoButton.add(newsLabel);

			statButton = Glazier.Get().CreateButton();
			//statButton.positionOffset_X = -100;
			statButton.PositionOffset_Y = 110;
			//			statButton.positionScale_X = 0.5f;
			//			statButton.positionScale_Y = 0.25f;
			//			statButton.sizeOffset_X = 400;
			statButton.SizeOffset_Y = 50;
			statButton.SizeScale_X = 1;
			statButton.OnClicked += onClickedStatButton;
			container.AddChild(statButton);
			stat = EPlayerStat.NONE;

			onClickedStatButton(statButton);
		}
	}
}
