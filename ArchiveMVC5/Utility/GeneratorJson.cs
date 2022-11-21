using ArchiveMVC5.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace ArchiveMVC5.Utility
{
    public class GeneratorJson
    {
        public static List<Dictionary<string, object>> GetJson(DataTable dt)
        {
            System.Web.Script.Serialization.JavaScriptSerializer serializer = new

            System.Web.Script.Serialization.JavaScriptSerializer();
            List<Dictionary<string, object>> rows =
              new List<Dictionary<string, object>>();
            Dictionary<string, object> row = null;

            foreach (DataRow dr in dt.Rows)
            {
                row = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {                    
                    row.Add(col.ColumnName.Trim().Replace("_4E19573E_D42E_4B74_BB81_E3EF56633947", ""), dr[col]);
                    if (col.ColumnName.Replace("_4E19573E_D42E_4B74_BB81_E3EF56633947", "").Equals("DocumentId"))
                    {
                        row.Add("DocumentIdEncrypt", Ecm.Utility.CryptographyHelper.EncryptUsingSymmetricAlgorithm(dr[col].ToString()));
                    }
                }
                rows.Add(row);
            }
            return rows;
            //return serializer.Serialize(rows);
        }
        
public static string CreateConjunction()
        {
            var sa = new Dictionary<int, String>();
            foreach (Ecm.Domain.SearchConjunction dataType in Enum.GetValues(typeof(Ecm.Domain.SearchConjunction)))
            {
                sa.Add((int)dataType, dataType.ToString());


            }            
            return "{Conjunctions:[" + String.Join(",", sa.Select(p => "{ID:" + p.Key + ",Name:'" + p.Value + "'}").ToArray()) + "]}";
        }

        public static string CreateDataType()
        {
            var sa = new Dictionary<int, String>();
            foreach (Ecm.Domain.FieldDataType dataType in Enum.GetValues(typeof(Ecm.Domain.FieldDataType)))
            {
                if (dataType != Ecm.Domain.FieldDataType.Folder && dataType != Ecm.Domain.FieldDataType.Table)
                {
                    sa.Add((int)dataType, dataType.ToString());
                }
            }             
            return "{DataTypes:["+String.Join(",", sa.Select(p => "{Type:" + p.Key + ",Name:'" + p.Value + "'}").ToArray())+"]}";
        }

        static string CreateOperatorItems_From_DataType(Ecm.Domain.FieldDataType dataType){
            String strOperatorItems= @"{{
                                            ID:{0},
                                            Name: '{1}',
                                            MumberControl: {2},
                                            Control: '{3}',
                                            IsDate: {4}
                                        }}";
            List<string> listOperatorItems = new List<string>();

            switch(dataType){
                case Ecm.Domain.FieldDataType.String:
                    listOperatorItems.Add(String.Format(strOperatorItems, (int)Ecm.Domain.SearchOperator.Contains, Ecm.Domain.SearchOperator.Contains, 1, "<input id=\"value1\" type=\"text\" value=\"some text\">  <button class=\"close\" tabindex=\"-1\">×</button>", 0));
                    listOperatorItems.Add(String.Format(strOperatorItems, (int)Ecm.Domain.SearchOperator.EndsWith, Ecm.Domain.SearchOperator.EndsWith, 1, "<input id=\"value1\" type=\"text\" value=\"some text\">  <button class=\"close\" tabindex=\"-1\">×</button>", 0));
                    listOperatorItems.Add(String.Format(strOperatorItems, (int)Ecm.Domain.SearchOperator.Equal, Ecm.Domain.SearchOperator.Equal, 1, "<input id=\"value1\" type=\"text\" value=\"some text\">  <button class=\"close\" tabindex=\"-1\">×</button>", 0));
                    listOperatorItems.Add(String.Format(strOperatorItems, (int)Ecm.Domain.SearchOperator.NotContains, Ecm.Domain.SearchOperator.NotContains, 1, "<input id=\"value1\" type=\"text\" value=\"some text\">  <button class=\"close\" tabindex=\"-1\">×</button>", 0));
                    listOperatorItems.Add(String.Format(strOperatorItems, (int)Ecm.Domain.SearchOperator.NotEqual, Ecm.Domain.SearchOperator.NotEqual, 1, "<input id=\"value1\" type=\"text\" value=\"some text\">  <button class=\"close\" tabindex=\"-1\">×</button>", 0));
                    listOperatorItems.Add(String.Format(strOperatorItems, (int)Ecm.Domain.SearchOperator.StartsWith, Ecm.Domain.SearchOperator.StartsWith, 1, "<input id=\"value1\" type=\"text\" value=\"some text\">  <button class=\"close\" tabindex=\"-1\">×</button>", 0));
                    break;
                case Ecm.Domain.FieldDataType.Picklist:
                    listOperatorItems.Add(String.Format(strOperatorItems, (int)Ecm.Domain.SearchOperator.Equal, Ecm.Domain.SearchOperator.Equal, 1, "<input id=\"value1\" type=\"text\" value=\"some text\">  <button class=\"close\" tabindex=\"-1\">×</button>", 0));
                    listOperatorItems.Add(String.Format(strOperatorItems, (int)Ecm.Domain.SearchOperator.NotEqual, Ecm.Domain.SearchOperator.NotEqual, 1, "<input id=\"value1\" type=\"text\" value=\"some text\">  <button class=\"close\" tabindex=\"-1\">×</button>", 0));
                    break;
                case Ecm.Domain.FieldDataType.Date:
                    listOperatorItems.Add(String.Format(strOperatorItems, (int)Ecm.Domain.SearchOperator.Equal, Ecm.Domain.SearchOperator.Equal, 1, "<input id=\"value1\" type=\"text\" value=\"\">", 1));
                    listOperatorItems.Add(String.Format(strOperatorItems, (int)Ecm.Domain.SearchOperator.InBetween, Ecm.Domain.SearchOperator.InBetween, 2, "<input id=\"value1\" type=\"text\" value=\"\">", 1));
                    listOperatorItems.Add(String.Format(strOperatorItems, (int)Ecm.Domain.SearchOperator.NotEqual, Ecm.Domain.SearchOperator.NotEqual, 1, "<input id=\"value1\" type=\"text\" value=\"\">", 1));
                    listOperatorItems.Add(String.Format(strOperatorItems, (int)Ecm.Domain.SearchOperator.GreaterThan, Ecm.Domain.SearchOperator.GreaterThan, 1, "<input id=\"value1\" type=\"text\" value=\"\">", 1));
                    listOperatorItems.Add(String.Format(strOperatorItems, (int)Ecm.Domain.SearchOperator.GreaterThanOrEqualTo, Ecm.Domain.SearchOperator.GreaterThanOrEqualTo, 1, "<input id=\"value1\" type=\"text\" value=\"\">", 1));
                    listOperatorItems.Add(String.Format(strOperatorItems, (int)Ecm.Domain.SearchOperator.LessThan, Ecm.Domain.SearchOperator.LessThan, 1, "<input id=\"value1\" type=\"text\" value=\"\">", 1));
                    listOperatorItems.Add(String.Format(strOperatorItems, (int)Ecm.Domain.SearchOperator.LessThanOrEqualTo, Ecm.Domain.SearchOperator.LessThanOrEqualTo, 1, "<input  id=\"value1\" type=\"text\" value=\"\">", 1));
                    break;
               case Ecm.Domain.FieldDataType.Decimal:
               case Ecm.Domain.FieldDataType.Integer:
                    listOperatorItems.Add(String.Format(strOperatorItems, (int)Ecm.Domain.SearchOperator.Equal, Ecm.Domain.SearchOperator.Equal, 1, "<input  id=\"value1\" type=\"text\" value=\"0\">  <button class=\"close\" tabindex=\"-1\">×</button>", 0));
                    listOperatorItems.Add(String.Format(strOperatorItems, (int)Ecm.Domain.SearchOperator.InBetween, Ecm.Domain.SearchOperator.InBetween, 2, "<input  id=\"value1\" type=\"text\" value=\"0\"> <button class=\"close\" tabindex=\"-1\">×</button>", 0));
                    listOperatorItems.Add(String.Format(strOperatorItems, (int)Ecm.Domain.SearchOperator.NotEqual, Ecm.Domain.SearchOperator.NotEqual, 1, "<input id=\"value1\" type=\"text\" value=\"0\">  <button class=\"close\" tabindex=\"-1\">×</button>", 0));
                    listOperatorItems.Add(String.Format(strOperatorItems, (int)Ecm.Domain.SearchOperator.GreaterThan, Ecm.Domain.SearchOperator.GreaterThan, 1, "<input id=\"value1\" type=\"text\" value=\"0\">  <button class=\"close\" tabindex=\"-1\">×</button>", 0));
                    listOperatorItems.Add(String.Format(strOperatorItems, (int)Ecm.Domain.SearchOperator.GreaterThanOrEqualTo, Ecm.Domain.SearchOperator.GreaterThanOrEqualTo, 1, "<input id=\"value1\" type=\"text\" value=\"0\">  <button class=\"close\" tabindex=\"-1\">×</button>", 0));
                    listOperatorItems.Add(String.Format(strOperatorItems, (int)Ecm.Domain.SearchOperator.LessThan, Ecm.Domain.SearchOperator.LessThan, 1, "<input id=\"value1\" type=\"text\" value=\"0\">  <button class=\"close\" tabindex=\"-1\">×</button>", 0));
                    listOperatorItems.Add(String.Format(strOperatorItems, (int)Ecm.Domain.SearchOperator.LessThanOrEqualTo, Ecm.Domain.SearchOperator.LessThanOrEqualTo, 1, "<input id=\"value1\" type=\"text\" value=\"0\">  <button class=\"close\" tabindex=\"-1\">×</button>", 0));
                    break;
            case Ecm.Domain.FieldDataType.Boolean:
                    listOperatorItems.Add(String.Format(strOperatorItems, (int)Ecm.Domain.SearchOperator.Equal, Ecm.Domain.SearchOperator.Equal, 1, "<input id=\"value1\" type=\"text\" value=\"some text\">  <button class=\"close\" tabindex=\"-1\">×</button>", 0));
                    listOperatorItems.Add(String.Format(strOperatorItems, (int)Ecm.Domain.SearchOperator.NotEqual, Ecm.Domain.SearchOperator.NotEqual, 1, "<input id=\"value1\" type=\"text\" value=\"some text\">  <button class=\"close\" tabindex=\"-1\">×</button>", 0));
                    break;
            }



            return String.Join(",", listOperatorItems);
        
        }

        public static string GetComboBoxValue(FieldMetaDataModel field, Ecm.Domain.SearchOperator searchOperator, string value1, string value2)
        {
            switch (field.DataType)
            {
                case Ecm.Domain.FieldDataType.String:
                    switch (searchOperator)
                    {
                        case Ecm.Domain.SearchOperator.Contains:
                        case Ecm.Domain.SearchOperator.EndsWith:
                        case Ecm.Domain.SearchOperator.Equal:
                        case Ecm.Domain.SearchOperator.NotContains:
                        case Ecm.Domain.SearchOperator.NotEqual:
                        case Ecm.Domain.SearchOperator.StartsWith:
                            return "<input id=\"value1\" type=\"text\" value=\"" + value1 + "\" class='form-control input-sm'>";
                    }
                    
                    break;
                case Ecm.Domain.FieldDataType.Picklist:
                    switch (searchOperator)
                    {
                        case Ecm.Domain.SearchOperator.Equal:
                        case Ecm.Domain.SearchOperator.NotEqual:
                            string returnVal = @"<select id='value1' class='form-control input-sm'>{0}</select>";
                            string opt = @"<option>{0}</option>";
                            string opts = @"<option></option>";

                            foreach (var pic in field.Picklists)
                            {
                                if (!string.IsNullOrEmpty(pic.Value))
                                {
                                    opts += string.Format(opt, pic.Value);
                                }
                            }

                            returnVal = string.Format(returnVal, opts);

                            return returnVal;
                    }
                    break;
                case Ecm.Domain.FieldDataType.Date:

                    switch (searchOperator)
                    {
                        case Ecm.Domain.SearchOperator.InBetween:
                            return @"<div class='col-md-12' style='padding-left: 0px !important'>" +
                                        "<div class='form-group col-md-5' style='padding-left: 0px !important; margin-bottom:1px !important;'>" +
                                            "<div class='input-group input-append date' data-date-format='mm-dd-yyyy'>" +
                                                "<input id='value1' type='text' class='form-control input-sm'>" +
                                                "<span class='input-group-addon add-on btn' id='dtpFromDate'><i class='fa fa-calendar'></i></span>" + 
                                            "</div>" +
                                        "</div>" +
                                        "<div class='form-group col-md-2' style='padding-left: 0px !important; margin-bottom:1px !important;'>" +
                                            "<span> - </span>" +
                                        "</div>" +
                                        "<div class='form-group col-md-5' style='padding-left: 0px !important; margin-bottom:1px !important;'>" +
                                            "<div class='input-group input-append date' data-date-format='mm-dd-yyyy'>" +
                                                "<input id='value2' type='text' class='form-control input-sm'>" +
                                                "<span class='input-group-addon add-on btn' id='dtpToDate'><i class='fa fa-calendar'></i></span>" + 
                                            "</div>" +
                                        "</div>" +
                                    "</div><script>$('#" + field.Id + "').find('#value1').datepicker(); $('#" + field.Id + "').find('#value2').datepicker();</script>";
                        case Ecm.Domain.SearchOperator.Equal:
                        case Ecm.Domain.SearchOperator.NotEqual:
                        case Ecm.Domain.SearchOperator.GreaterThan:
                        case Ecm.Domain.SearchOperator.GreaterThanOrEqualTo:
                        case Ecm.Domain.SearchOperator.LessThan:
                        case Ecm.Domain.SearchOperator.LessThanOrEqualTo:
                             //"<div class=\"input-group input-append date\" ><input  id=\"value1\" type=\"text\" value=\"" + value1 + "\" class='form-control input-sm'></div>";
                            return @"<div class='input-group input-append date' data-date-format='dd-mm-yyyy'>" +
                                        "<input id='value1' type='text' class='form-control input-sm'>" +
                                        "<span class='input-group-addon add-on btn' id='dtpFromDate'><i class='fa fa-calendar'></i></span>" +
                                    "</div><script>$('#" + field.Id + "').find('#value1').datepicker();</script>";

                    }
                    break;
                    
                case Ecm.Domain.FieldDataType.Decimal:
                case Ecm.Domain.FieldDataType.Integer:
                    switch (searchOperator)
                    {
                        case Ecm.Domain.SearchOperator.InBetween:
                            return @"<div class='col-md-12' style='padding-left: 0px !important'>" +
                                        "<div class='form-group col-md-5' style='padding-left: 0px !important; margin-bottom:1px !important;'>" +
                                            "<div class='input-group input-append number'>" +
                                                "<input id='value1' type='text' class='form-control input-sm'>"+
                                            "</div>" +
                                        "</div>" +
                                        "<div class='form-group col-md-2' style='padding-left: 0px !important; margin-bottom:1px !important;'>" +
                                            "<span> - </span>" +
                                        "</div>" +
                                        "<div class='form-group col-md-5' style='padding-left: 0px !important; margin-bottom:1px !important;'>" +
                                            "<div class='input-group input-append number'>" +
                                                "<input id='value2' type='text' class='form-control input-sm'>" +
                                            "</div>" +
                                        "</div>" +
                                    "</div>";
                        case Ecm.Domain.SearchOperator.Equal:
                        case Ecm.Domain.SearchOperator.NotEqual:
                        case Ecm.Domain.SearchOperator.GreaterThan:
                        case Ecm.Domain.SearchOperator.GreaterThanOrEqualTo:
                        case Ecm.Domain.SearchOperator.LessThan:
                        case Ecm.Domain.SearchOperator.LessThanOrEqualTo:
                            return "<input id=\"value1\" type=\"text\" value=\"" + value1 + "\" class='form-control input-sm'>";
                    }
                    break;
                case Ecm.Domain.FieldDataType.Boolean:
                    switch (searchOperator)
                    {
                    case Ecm.Domain.SearchOperator.Equal:
                            return "<div class=\"input-group input-append \" ><input id=\"value1\" type=\"text\" value=\"" + value1 + "\" class='form-control input-sm'></div>";
                        case Ecm.Domain.SearchOperator.NotEqual:
                            return "<div class=\"input-group input-append \" ><input id=\"value1\" type=\"text\" value=\"" + value1 + "\" class='form-control input-sm'></div>";
                    }
                    break;
            }

            return "";

        }

        public static List<Ecm.Domain.SearchOperator> GetOperatorFromType(Ecm.Domain.FieldDataType dataType)
        {

            List<Ecm.Domain.SearchOperator> listOperatorItems = new List<Ecm.Domain.SearchOperator>();

            switch (dataType)
            {
                case Ecm.Domain.FieldDataType.String:
                    listOperatorItems.Add(Ecm.Domain.SearchOperator.Contains);
                    listOperatorItems.Add(Ecm.Domain.SearchOperator.EndsWith);
                    listOperatorItems.Add(Ecm.Domain.SearchOperator.Equal);
                    listOperatorItems.Add(Ecm.Domain.SearchOperator.NotContains);
                    listOperatorItems.Add(Ecm.Domain.SearchOperator.NotEqual);
                    listOperatorItems.Add(Ecm.Domain.SearchOperator.StartsWith);
                    break;
                case Ecm.Domain.FieldDataType.Picklist:
                    listOperatorItems.Add(Ecm.Domain.SearchOperator.Equal);
                    listOperatorItems.Add(Ecm.Domain.SearchOperator.NotEqual);
                    break;
                case Ecm.Domain.FieldDataType.Date:
                    listOperatorItems.Add(Ecm.Domain.SearchOperator.Equal);
                    listOperatorItems.Add(Ecm.Domain.SearchOperator.InBetween);
                    listOperatorItems.Add(Ecm.Domain.SearchOperator.NotEqual);
                    listOperatorItems.Add(Ecm.Domain.SearchOperator.GreaterThan);
                    listOperatorItems.Add(Ecm.Domain.SearchOperator.GreaterThanOrEqualTo);
                    listOperatorItems.Add(Ecm.Domain.SearchOperator.LessThan);
                    listOperatorItems.Add(Ecm.Domain.SearchOperator.LessThanOrEqualTo);
                    break;
                case Ecm.Domain.FieldDataType.Decimal:
                case Ecm.Domain.FieldDataType.Integer:
                    listOperatorItems.Add(Ecm.Domain.SearchOperator.Equal);
                    listOperatorItems.Add(Ecm.Domain.SearchOperator.InBetween);
                    listOperatorItems.Add(Ecm.Domain.SearchOperator.NotEqual);
                    listOperatorItems.Add(Ecm.Domain.SearchOperator.GreaterThan);
                    listOperatorItems.Add(Ecm.Domain.SearchOperator.GreaterThanOrEqualTo);
                    listOperatorItems.Add(Ecm.Domain.SearchOperator.LessThan);
                    listOperatorItems.Add(Ecm.Domain.SearchOperator.LessThanOrEqualTo);
                    break;
                case Ecm.Domain.FieldDataType.Boolean:
                    listOperatorItems.Add(Ecm.Domain.SearchOperator.Equal);
                    listOperatorItems.Add(Ecm.Domain.SearchOperator.NotEqual);
                    break;
            }



            return listOperatorItems;
        }


        static string CreateDataType_Item_From_DataType(Ecm.Domain.FieldDataType dataType){
            String strData_Item= @"{{
                                        TypeID:{0},
                                        TypeName:'{1}',
                                        OperatorItems: [{2}]
                                    }}";
            


            return String.Format(strData_Item,
                                (int)dataType,
                                dataType.ToString(),
                                CreateOperatorItems_From_DataType(dataType)
                                 );
        }

        public static string CreateDataType_Item()
        {
            String strDataType=@"{{DataType_Items:[
                                                    {0}
                                                    ]
                                    }}";

            List<string> list_item = new List<string>();
            foreach (Ecm.Domain.FieldDataType dataType in Enum.GetValues(typeof(Ecm.Domain.FieldDataType)))
            {
                //sa.Add((int)dataType, dataType.ToString());
                if (dataType != Ecm.Domain.FieldDataType.Table && dataType != Ecm.Domain.FieldDataType.Folder)
                {
                    list_item.Add(CreateDataType_Item_From_DataType(dataType));
                }
            }
            return String.Format(strDataType,String.Join(",", list_item));

            
        }
    }
}