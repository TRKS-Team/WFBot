using System.Collections.Generic;
using Newbe.Mahua;

namespace TRKS.WF.QQBot
{
    public class MyMenuProvider : IMahuaMenuProvider
    {
        public IEnumerable<MahuaMenu> GetMenus()
        {
            return new[]
            {
                new MahuaMenu
                {
                    Id = "menu1",
                    Text = "修改机器人访问口令(用于修改群)"
                }
            };
        }
    }
}
