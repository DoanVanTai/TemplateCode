using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DVTools
{
    public static class GetFamilyName
    {
        public static List<string> Beam = new List<string>
        {
           "Revgen - Bjælke - KB", "Revgen - Bjælke - KBE"
        };
        
        public static string BeamT = "Revgen - Bjælke - KB";

        public static string BeamL = "Revgen - Bjælke - KBE";

        public static List<string> ColumnRec = new List<string>
        {
            "Revgen-Column-SE-H", "RevgenCorbelPillar"
        };
    }
     
    public static class GetParameterBeamT
    {
        //public static string B_Height = "Height";
        //public static string B_HeightLedge1 = "Height Ledge 1";
        //public static string B_HeightLedge2 = "Height Ledge 2";
        public static string B_Width = "Width";
        //public static string B_WidthLedge1 = "Width Ledge 1";
        //public static string B_WidthLedge2 = "Width Ledge 2";
    }
    public static class GetParameterColumnRec
    {
        public static string C_Width = "Width";
        public static string C_Depth = "Depth";
       
    }

}



