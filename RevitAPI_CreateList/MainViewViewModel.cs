using Autodesk.Revit.DB;

using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAPI_CreateList
{
    public class MainViewViewModel
    {
        private ExternalCommandData _commandData;
        public DelegateCommand CreateLists { get; }


        public List<Element> ListsTypes { get; } = new List<Element>();

        public Element SelectedListsType { get; set; }
        public List<ViewPlan> Views { get; private set; } = new List<ViewPlan>();
        public ViewPlan SelectedView { get; set; }

        public string Autor { get; set; } = string.Empty;
        public int ListsCount { get; set; } = 0;

        public MainViewViewModel(ExternalCommandData commandData)
        {
            _commandData = commandData;

            UIApplication uiapp = _commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            CreateLists = new DelegateCommand(OnCreateLists);

            
            ListsTypes = new FilteredElementCollector(doc)
                 .OfCategory(BuiltInCategory.OST_TitleBlocks)
                 .Cast<Element>()
                 .ToList();

            Views = new FilteredElementCollector(doc)
                   .OfClass(typeof(ViewPlan))
                   .Cast<ViewPlan>()
                   .Where(p => p.ViewType == ViewType.FloorPlan)
                   .ToList();
                    }

        private void OnCreateLists()
        {
            RaiseCloseRequest();
            UIApplication uiapp = _commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            using (Transaction trans = new Transaction(doc, "Изменение типа стен"))
            {
                trans.Start();
                for (int i = 0; i < ListsCount; i++)
                {
                    ViewSheet viewSheet = ViewSheet.Create(doc, SelectedListsType.Id);

                    UV location = new UV((viewSheet.Outline.Max.U - viewSheet.Outline.Min.U) / 2,
                                     (viewSheet.Outline.Max.V - viewSheet.Outline.Min.V) / 2);

                    var newView=doc.GetElement(SelectedView.Duplicate(ViewDuplicateOption.Duplicate)) as ViewPlan;

                    newView.Name = $"Копия {(i + 1).ToString()} " + SelectedView.Name;
                    Viewport.Create(doc, viewSheet.Id, newView.Id, new XYZ(location.U,location.V,0));

                    Parameter autor = viewSheet.get_Parameter(BuiltInParameter.SHEET_DESIGNED_BY);
                    autor.Set(Autor);

                }               
                
                trans.Commit();
            }
        }



        public event EventHandler CloseRequest;
        private void RaiseCloseRequest()
        {
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }

    }
}
