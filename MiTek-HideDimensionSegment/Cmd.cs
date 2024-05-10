using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using System.Runtime.Remoting.Messaging;
using System.Windows.Input;

namespace MiTek_HideDimensionSegment
{
    [Transaction(TransactionMode.Manual)]
    public class Cmd : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        UIApplication uiapp = commandData.Application;
        UIDocument uidoc = uiapp.ActiveUIDocument;
        Application app = uiapp.Application;
        Document doc = uidoc.Document;
        do
        {
            Selection sel = uidoc.Selection;
            // Chọn một đối tượng Dimension
            Reference dimensionRef = sel.PickObject(ObjectType.Element, "Chọn một đối tượng Dimension");
            Dimension dimension = doc.GetElement(dimensionRef) as Dimension;

            // Kiểm tra xem đối tượng đã chọn có phải là Dimension không
            if (dimension == null)
            {
                TaskDialog.Show("Lỗi", "Đối tượng chọn không phải là Dimension.");
                break;
            }
            if (dimension.NumberOfSegments == 0)
            {

                using (Transaction transaction = new Transaction(doc, "Ghi đè giá trị cho DimensionSegment"))
                {
                    transaction.Start();

                    // Ghi đè giá trị cho DimensionSegment gần nhất
                    dimension.ValueOverride = "\u001f";

                    // Kết thúc giao dịch
                    transaction.Commit();
                }
            }
            else
            {
                // Chọn một điểm bất kỳ
                XYZ pickedPoint = sel.PickPoint("Chọn một điểm bất kỳ");

                // Lấy list DimensionSegmentArray
                DimensionSegment nearestSegment = null;
                double minDistance = double.MaxValue;
                foreach (DimensionSegment segment in dimension.Segments)
                {
                    XYZ segmentOrigin = segment.Origin;

                    double distance = pickedPoint.DistanceTo(segmentOrigin);

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearestSegment = segment;
                    }
                }
                if (nearestSegment != null)
                {
                    // Bắt đầu một giao dịch để thực hiện các thay đổi
                    using (Transaction transaction = new Transaction(doc, "Ghi đè giá trị cho DimensionSegment"))
                    {
                        transaction.Start();

                        // Ghi đè giá trị cho DimensionSegment gần nhất
                        nearestSegment.ValueOverride = "\u001f"; // Thay thế "New Value" bằng giá trị mới

                        // Kết thúc giao dịch
                        transaction.Commit();
                    }
                }
            }

        } while (true);
        return Result.Succeeded;
    }
}
}
