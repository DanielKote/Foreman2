using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Foreman
{
	public class RecipeNodeElement : BaseNodeElement
	{
		protected override Brush CleanBgBrush { get { return recipeBgBrush; } }
		private static readonly Brush recipeBgBrush = new SolidBrush(Color.FromArgb(190, 217, 212));
		private static readonly Pen productivityPen = new Pen(Brushes.DarkRed, 6);
		private static readonly Pen productivityPlusPen = new Pen(productivityPen.Brush, 2);
		private static readonly Pen extraProductivityPen = new Pen(Brushes.Crimson, 6);

		private static readonly StringFormat textFormat = new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };

		private readonly AssemblerElement AssemblerElement;
		private readonly BeaconElement BeaconElement;

		private string RecipeName { get { return DisplayedNode.BaseRecipe.FriendlyName; } }

		private new readonly ReadOnlyRecipeNode DisplayedNode;

		private static bool OptionsCopyAssemblerDefault = true;
		private static bool OptionsCopyExtraProductivityMinersDefault = true;
		private static bool OptionsCopyExtraProductivityNonMinersDefault = true;
		private static bool OptionsCopyFuelDefault = true;
		private static bool OptionsCopyModulesDefault = true;
		private static bool OptionsCopyBeaconDefault = true;
		private static bool OptionsCopyBeaconModulesDefault = true;

		public RecipeNodeElement(ProductionGraphViewer graphViewer, ReadOnlyRecipeNode node) : base(graphViewer, node)
		{
			DisplayedNode = node;

			AssemblerElement = new AssemblerElement(graphViewer, this);
			AssemblerElement.SetVisibility(graphViewer.LevelOfDetail != ProductionGraphViewer.LOD.Low);

			BeaconElement = new BeaconElement(graphViewer, this);
			BeaconElement.SetVisibility(graphViewer.LevelOfDetail != ProductionGraphViewer.LOD.Low);

			UpdateState();
		}

		protected override void UpdateState()
		{
			int yOffset = (DisplayedNode.NodeDirection == NodeDirection.Up && InputTabs.Count == 0 && OutputTabs.Count != 0) || (DisplayedNode.NodeDirection == NodeDirection.Down && OutputTabs.Count == 0 && InputTabs.Count != 0) ? 10 :
				(DisplayedNode.NodeDirection == NodeDirection.Down && InputTabs.Count == 0 && OutputTabs.Count != 0) || (DisplayedNode.NodeDirection == NodeDirection.Up && OutputTabs.Count == 0 && InputTabs.Count != 0) ? -10 : 0;
			yOffset += DisplayedNode.NodeDirection == NodeDirection.Down ? 4 : 0;

			AssemblerElement.Location = new Point(-26, -14 + yOffset);
			BeaconElement.Location = new Point(-30, 27 + yOffset);

			AssemblerElement.SetVisibility(graphViewer.LevelOfDetail != ProductionGraphViewer.LOD.Low);
			BeaconElement.SetVisibility(graphViewer.LevelOfDetail != ProductionGraphViewer.LOD.Low);

			Width = Math.Max(MinWidth, Math.Max(GetIconWidths(InputTabs), GetIconWidths(OutputTabs)) + 10);
			if (Width % WidthD != 0)
			{
				Width += WidthD;
				Width -= Width % WidthD;
			}
			Height = (graphViewer.LevelOfDetail == ProductionGraphViewer.LOD.Low) ? BaseSimpleHeight : BaseRecipeHeight;

			//update tabs (necessary now that it is possible that an item was added or removed)... I am looking at you furnaces!!!
			//done by first checking all old tabs and removing any that are no longer part of the displayed node, then looking at the displayed node io and adding any new tabs that are necessary.
			//could potentially be done by just deleting all the old ones and remaking them from scratch, but come on - thats much more intensive than just doing some checks!
			foreach (ItemTabElement oldTab in InputTabs.Where(tab => !DisplayedNode.Inputs.Contains(tab.Item)).ToList())
			{
				foreach (ReadOnlyNodeLink link in DisplayedNode.InputLinks.Where(link => link.Item == oldTab.Item).ToList())
					graphViewer.Graph.DeleteLink(link);
				InputTabs.Remove(oldTab);
				oldTab.Dispose();
			}
			foreach (ItemTabElement oldTab in OutputTabs.Where(tab => !DisplayedNode.Outputs.Contains(tab.Item)).ToList())
			{
				foreach (ReadOnlyNodeLink link in DisplayedNode.OutputLinks.Where(link => link.Item == oldTab.Item).ToList())
					graphViewer.Graph.DeleteLink(link);
				OutputTabs.Remove(oldTab);
				oldTab.Dispose();
			}
			foreach (Item item in DisplayedNode.Inputs)
				if (!InputTabs.Any(tab => tab.Item == item))
					InputTabs.Add(new ItemTabElement(item, LinkType.Input, graphViewer, this));
			foreach (Item item in DisplayedNode.Outputs)
				if (!OutputTabs.Any(tab => tab.Item == item))
					OutputTabs.Add(new ItemTabElement(item, LinkType.Output, graphViewer, this));

			base.UpdateState();
		}

		protected override Bitmap NodeIcon() { return DisplayedNode.BaseRecipe.Icon; }

		protected override void DetailsDraw(Graphics graphics, Point trans)
		{
			if (graphViewer.LevelOfDetail == ProductionGraphViewer.LOD.Low) //text only view
			{
				//text
				bool oversupplied = DisplayedNode.IsOversupplied();
				Rectangle textSlot = new Rectangle(trans.X - (Width / 2) + 40, trans.Y - (Height / 2) + (oversupplied ? 32 : 27), (Width - 10 - 40), Height - (oversupplied ? 64 : 54));
				//graphics.DrawRectangle(devPen, textSlot);
				int textLength = GraphicsStuff.DrawText(graphics, TextBrush, textFormat, RecipeName, BaseFont, textSlot);

				//assembler icon
				graphics.DrawImage(DisplayedNode.SelectedAssembler == null ? DataCache.UnknownIcon : DisplayedNode.SelectedAssembler.Icon, trans.X - Math.Min((Width / 2) - 10, (textLength / 2) + 32), trans.Y - 16, 32, 32);

				//productivity ticks
				int pModules = DisplayedNode.AssemblerModules.Count(m => m.ProductivityBonus > 0);
				pModules += (int)(DisplayedNode.BeaconModules.Count(m => m.ProductivityBonus > 0) * DisplayedNode.BeaconCount);

				bool extraProductivity = DisplayedNode.ExtraProductivity > 0 && (DisplayedNode.SelectedAssembler.EntityType == EntityType.Miner || graphViewer.Graph.EnableExtraProductivityForNonMiners);
				pModules += extraProductivity ? 1 : 0;

				for (int i = 0; i < pModules && i < 6; i++)
					graphics.DrawEllipse((extraProductivity && i == 0) ? extraProductivityPen : productivityPen, trans.X - (Width / 2) - 1, trans.Y - (Height / 2) + 10 + i * 12, 6, 6);
				if (pModules > 6)
				{
					graphics.DrawLine(productivityPlusPen, trans.X - (Width / 2) - 4, trans.Y - (Height / 2) + 84, trans.X - (Width / 2) + 8, trans.Y - (Height / 2) + 84);
					graphics.DrawLine(productivityPlusPen, trans.X - (Width / 2) + 2, trans.Y - (Height / 2) + 84 - 6, trans.X - (Width / 2) + 2, trans.Y - (Height / 2) + 84 + 6);
				}
			}
			else if (DisplayedNode.ExtraProductivity > 0 && (DisplayedNode.SelectedAssembler.EntityType == EntityType.Miner || graphViewer.Graph.EnableExtraProductivityForNonMiners))
			{
				graphics.DrawEllipse(extraProductivityPen, trans.X - (Width / 2) - 1, trans.Y - (Height / 2) + 10, 6, 6);
			}
		}

		protected override void AddRClickMenuOptions(bool nodeInSelection)
		{
			if (nodeInSelection)
			{
				List<ReadOnlyRecipeNode> rNodes = new List<ReadOnlyRecipeNode>(graphViewer.SelectedNodes.Where(ne => ne is RecipeNodeElement).Select(ne => (ReadOnlyRecipeNode)ne.DisplayedNode));
				if (!rNodes.Contains(this.DisplayedNode))
					rNodes.Add((ReadOnlyRecipeNode)this.DisplayedNode);

				RightClickMenu.Items.Add(new ToolStripSeparator());

				RightClickMenu.Items.Add(new ToolStripMenuItem("Apply default assembler(s)", null,
					new EventHandler((o, e) =>
					{
						RightClickMenu.Close();
						foreach (ReadOnlyRecipeNode rNode in rNodes)
							((RecipeNodeController)graphViewer.Graph.RequestNodeController(rNode)).AutoSetAssembler();
					})));
				RightClickMenu.Items.Add(new ToolStripMenuItem("Apply default modules", null,
					new EventHandler((o, e) =>
					{
						RightClickMenu.Close();
						foreach (ReadOnlyRecipeNode rNode in rNodes)
							((RecipeNodeController)graphViewer.Graph.RequestNodeController(rNode)).AutoSetAssemblerModules();
					})));
				if (rNodes.Any(rn => rn.AssemblerModules.Count > 0))
					RightClickMenu.Items.Add(new ToolStripMenuItem("Remove modules", null,
						new EventHandler((o, e) =>
						{
							RightClickMenu.Close();
							foreach (ReadOnlyRecipeNode rNode in rNodes)
								((RecipeNodeController)graphViewer.Graph.RequestNodeController(rNode)).SetAssemblerModules(null, false);
						})));
				if (rNodes.Any(rn => rn.SelectedBeacon != null))
					RightClickMenu.Items.Add(new ToolStripMenuItem("Remove beacons", null,
						new EventHandler((o, e) =>
						{
							RightClickMenu.Close();
							foreach (ReadOnlyRecipeNode rNode in rNodes)
								((RecipeNodeController)graphViewer.Graph.RequestNodeController(rNode)).SetBeacon(null);
						})));

				RightClickMenu.Items.Add(new ToolStripSeparator());
				NodeCopyOptions copiedOptions = NodeCopyOptions.GetNodeCopyOptions(Clipboard.GetText(), graphViewer.DCache);
				if (copiedOptions != null)
				{
					bool canPasteAssembler = rNodes.Any(rn => rn.BaseRecipe.Assemblers.Contains(copiedOptions.Assembler));
					bool canPasteExtraProductivityMiners = rNodes.Any(rn => rn.SelectedAssembler.EntityType == EntityType.Miner);
					bool canPasteExtraProductivityNonMiners = graphViewer.Graph.EnableExtraProductivityForNonMiners && rNodes.Any(rn => rn.SelectedAssembler.EntityType != EntityType.Miner);
					bool canPasteFuel = copiedOptions.Fuel != null && (canPasteAssembler || rNodes.Any(rn => rn.BaseRecipe.Assemblers.Any(a => a.Fuels.Contains(copiedOptions.Fuel))));
					bool canPasteModules = copiedOptions.AssemblerModules.Count > 0 && (canPasteAssembler || rNodes.Any(rn => rn.BaseRecipe.Modules.Count > 0 && rn.SelectedAssembler.Modules.Count > 0 && rn.SelectedAssembler.ModuleSlots > 0));
					bool canPasteBeacon = copiedOptions.Beacon != null && (canPasteAssembler || rNodes.Any(rn => rn.BaseRecipe.Modules.Count > 0 && rn.SelectedAssembler.Modules.Count > 0));

					if (canPasteAssembler || canPasteFuel || canPasteModules || canPasteBeacon)
					{
						RightClickMenu.ShowCheckMargin = true;

						ToolStripMenuItem assemblerCheck = new ToolStripMenuItem(copiedOptions.Assembler.GetEntityTypeName(false)) { CheckOnClick = true, Checked = canPasteAssembler && OptionsCopyAssemblerDefault, Enabled = canPasteAssembler, Tag = "CheckBox" };
						ToolStripMenuItem extraProductivityMinersCheck = new ToolStripMenuItem("Bonus Productivity (Miners)") { CheckOnClick = true, Checked = canPasteExtraProductivityMiners && OptionsCopyExtraProductivityMinersDefault, Enabled = canPasteExtraProductivityMiners, Tag = "CheckBox" };
						ToolStripMenuItem extraProductivityNonMinersCheck = new ToolStripMenuItem("Bonus Productivity (non-Miners)") { CheckOnClick = true, Checked = canPasteExtraProductivityNonMiners && OptionsCopyExtraProductivityNonMinersDefault, Enabled = canPasteExtraProductivityNonMiners, Tag = "CheckBox" };
						ToolStripMenuItem fuelCheck = new ToolStripMenuItem("Fuel") { CheckOnClick = true, Checked = canPasteFuel && OptionsCopyFuelDefault, Enabled = canPasteFuel, Tag = "CheckBox" };
						ToolStripMenuItem modulesCheck = new ToolStripMenuItem("Modules") { CheckOnClick = true, Checked = canPasteModules && OptionsCopyModulesDefault, Enabled = canPasteModules, Tag = "CheckBox" };
						ToolStripMenuItem beaconCheck = new ToolStripMenuItem("Beacon") { CheckOnClick = true, Checked = canPasteBeacon && OptionsCopyBeaconDefault, Enabled = canPasteBeacon, Tag = "CheckBox" };
						ToolStripMenuItem beaconModuleCheck = new ToolStripMenuItem("Beacon Modules") { CheckOnClick = true, Checked = canPasteBeacon && OptionsCopyBeaconModulesDefault, Enabled = canPasteBeacon, Tag = "CheckBox" };

						if (canPasteAssembler) RightClickMenu.Items.Add(assemblerCheck);
						if (canPasteExtraProductivityMiners) RightClickMenu.Items.Add(extraProductivityMinersCheck);
						if (canPasteExtraProductivityNonMiners) RightClickMenu.Items.Add(extraProductivityNonMinersCheck);
						if (canPasteFuel) RightClickMenu.Items.Add(fuelCheck);
						if (canPasteModules) RightClickMenu.Items.Add(modulesCheck);
						if (canPasteBeacon) RightClickMenu.Items.Add(beaconCheck);
						if (canPasteBeacon) RightClickMenu.Items.Add(beaconModuleCheck);
						RightClickMenu.Items.Add(new ToolStripSeparator());
						RightClickMenu.Items.Add(new ToolStripMenuItem("Paste selected options", null,
							new EventHandler((o, e) =>
							{
								RightClickMenu.Close();
								if (canPasteAssembler) OptionsCopyAssemblerDefault = assemblerCheck.Checked;
								if (canPasteExtraProductivityMiners) OptionsCopyExtraProductivityMinersDefault = extraProductivityMinersCheck.Checked;
								if (canPasteExtraProductivityNonMiners) OptionsCopyExtraProductivityNonMinersDefault = extraProductivityNonMinersCheck.Checked;
								if (canPasteFuel) OptionsCopyFuelDefault = fuelCheck.Checked;
								if (canPasteModules) OptionsCopyModulesDefault = modulesCheck.Checked;
								if (canPasteBeacon) OptionsCopyBeaconDefault = beaconCheck.Checked;
								if (canPasteBeacon) OptionsCopyBeaconModulesDefault = beaconCheck.Checked;

								foreach (ReadOnlyRecipeNode rNode in rNodes)
								{
									RecipeNodeController controller = (RecipeNodeController)graphViewer.Graph.RequestNodeController(rNode);

									bool assemblerFilter = !assemblerCheck.Checked; //if we do copy assembler, then all the other options are copied only if the assembler is. If we do not copy assembler, then paste options to everyone
									if (assemblerCheck.Checked && rNode.BaseRecipe.Assemblers.Contains(copiedOptions.Assembler)) //assembler fits the given recipe
									{
										controller.SetAssembler(copiedOptions.Assembler);
										assemblerFilter = true;
										if (rNode.SelectedAssembler.EntityType == EntityType.Reactor)
											controller.SetNeighbourCount(copiedOptions.NeighbourCount);
									}

									if (extraProductivityMinersCheck.Checked && rNode.SelectedAssembler.EntityType == EntityType.Miner)
										controller.SetExtraProductivityBonus(copiedOptions.ExtraProductivityBonus);
									if (extraProductivityNonMinersCheck.Checked && rNode.SelectedAssembler.EntityType != EntityType.Miner)
										controller.SetExtraProductivityBonus(copiedOptions.ExtraProductivityBonus);


									if (fuelCheck.Checked && rNode.SelectedAssembler.Fuels.Contains(copiedOptions.Fuel)) //fuel fits the given recipe node
									controller.SetFuel(copiedOptions.Fuel);

									if (modulesCheck.Checked)
									{
										HashSet<Module> acceptableAssemblerModules = new HashSet<Module>(rNode.BaseRecipe.Modules.Intersect(rNode.SelectedAssembler.Modules));
										if (!copiedOptions.AssemblerModules.Any(module => !acceptableAssemblerModules.Contains(module))) //all modules we copied can be added to the selected recipe/assembler
										controller.SetAssemblerModules(copiedOptions.AssemblerModules, true);
									}

									if (beaconCheck.Checked && rNode.BaseRecipe.Modules.Intersect(rNode.SelectedAssembler.Modules).Count() > 0)
									{
										controller.SetBeacon(copiedOptions.Beacon);
										controller.SetBeaconCount(copiedOptions.BeaconCount);
										controller.SetBeaconsCont(copiedOptions.BeaconsConst);
										controller.SetBeaconsPerAssembler(copiedOptions.BeaconsPerAssembler);
									}

									if (beaconModuleCheck.Checked && rNode.SelectedBeacon != null)
									{
										HashSet<Module> acceptableBeaconModules = new HashSet<Module>(rNode.BaseRecipe.Modules.Intersect(rNode.SelectedAssembler.Modules).Intersect(rNode.SelectedBeacon.Modules));
										if (!copiedOptions.BeaconModules.Any(module => !acceptableBeaconModules.Contains(module)))
											controller.SetBeaconModules(copiedOptions.BeaconModules, true);
									}
								}
							})));

						RightClickMenu.Items.Add(new ToolStripSeparator());
					}
				}
			}
			else
				RightClickMenu.Items.Add(new ToolStripSeparator());

			RightClickMenu.Items.Add(new ToolStripMenuItem("Copy this assembler's options", null,
				new EventHandler((o, e) =>
				{
					RightClickMenu.Close();
					StringBuilder stringBuilder = new StringBuilder();
					var writer = new JsonTextWriter(new StringWriter(stringBuilder));

					JsonSerializer serialiser = JsonSerializer.Create();
					serialiser.Formatting = Formatting.None;
					serialiser.Serialize(writer, new NodeCopyOptions(DisplayedNode as ReadOnlyRecipeNode));

					Clipboard.SetText(stringBuilder.ToString());

				})));
		}

		protected override List<TooltipInfo> GetMyToolTips(Point graph_point, bool exclusive)
		{
			List<TooltipInfo> tooltips = new List<TooltipInfo>();

			if (graphViewer.ShowRecipeToolTip)
			{
				Recipe[] Recipes = new Recipe[] { DisplayedNode.BaseRecipe };
				TooltipInfo ttiRecipe = new TooltipInfo();
				ttiRecipe.Direction = Direction.Left;
				ttiRecipe.ScreenLocation = graphViewer.GraphToScreen(LocalToGraph(new Point(Width / 2, 0)));
				ttiRecipe.ScreenSize = RecipePainter.GetSize(Recipes);
				ttiRecipe.CustomDraw = new Action<Graphics, Point>((Graphics g, Point offset) => { RecipePainter.Paint(Recipes, g, offset); });
				tooltips.Add(ttiRecipe);
			}

			if (exclusive)
			{
				TooltipInfo helpToolTipInfo = new TooltipInfo();
				helpToolTipInfo.Text = string.Format("Left click on this node to edit its {0}, modules, beacon, etc.\nRight click for options.", DisplayedNode.SelectedAssembler.GetEntityTypeName(false).ToLower());
				helpToolTipInfo.Direction = Direction.None;
				helpToolTipInfo.ScreenLocation = new Point(10, 10);
				tooltips.Add(helpToolTipInfo);
			}

			return tooltips;
		}
	}
}
