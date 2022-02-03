using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestFunc {
    class SetQuery {
        private enum TYPE_PERMISSION {
            VISIBLE_NICNAME = 1
            , VISIBLE_NAME
            , VISIBLE_EMAIL
            , VISIBLE_AWATAR
        }

        private static int _type_object = 1
            , _id_user = 1;

        private string _value;

        public SetQuery ()
        {
            int id_rec = -1;

            _value = create (new List<object> () {
                    new Tuple<List<string []>, string>(new List<string[]> { new string[] { "`VALUE`", "0" }, new string [] { "`VALUE1`", "10000" } }
                        , string.Format($"`ID`={++id_rec} AND `ID_OBJECT`={_id_user} AND `TYPE`={(int)TYPE_PERMISSION.VISIBLE_NICNAME}")
                    )
                    , new Tuple<List<string []>, string>(new List<string[]> { new string[] { "`VALUE`", "1" } }
                        , string.Format($"`ID`={++id_rec} AND `ID_OBJECT`={_id_user} AND `TYPE`={(int)TYPE_PERMISSION.VISIBLE_NAME}")
                    )
                    , new Tuple<List<string []>, string>(new List<string[]> { new string[] { "`VALUE`", "1" } }
                        , string.Format($"`ID`={++id_rec} AND `ID_OBJECT`={_id_user} AND `TYPE`={(int)TYPE_PERMISSION.VISIBLE_EMAIL}")
                    )
                    , new Tuple<List<string []>, string>(new List<string[]> { new string[] { "`VALUE`", "0" } }
                        , string.Format($"`ID`={++id_rec} AND `ID_OBJECT`={_id_user} AND `TYPE`={(int)TYPE_PERMISSION.VISIBLE_AWATAR}")
                    )
                }
                , "`NAME_TABLE`"
                , string.Format ($"`TYPE_OBJECT`={_type_object}")
            );

            Console.WriteLine ($"{_value}");
        }

        private string create(List<object> items, string table, string where)
        {
            string strRes = string.Empty
                , strSet = string.Empty
                , strWhere = string.Empty;

            foreach (Tuple<List<string []>, string> item in items) {
                strSet = string.Empty;
                strWhere = string.Empty;

                foreach (string[] pair in item.Item1) {
                    strSet = string.Format ("{0}{1}{2}={3}"
                        , strSet
                        , string.IsNullOrEmpty(strSet) == false ? ", " : string.Empty
                        , pair[0]
                        , pair[1]);
                }

                strWhere = string.Format("{0}{1}{2}"
                    , where
                    , ((string.IsNullOrEmpty(where) == false) && (string.IsNullOrEmpty (item.Item2) == false)) ? " AND " : string.Empty
                    , item.Item2
                );

                strRes = string.Format("{0}UPDATE {1} SET {2} WHERE {3};{4}"
                    , strRes
                    , table
                    , strSet
                    , strWhere
                    , Environment.NewLine
                );
            }

            return strRes;
        }
    }
}
