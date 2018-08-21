using Newbe.Mahua.MahuaEvents;
using System;
using System.Threading;
using Newbe.Mahua;

namespace TRKS.WF.QQBot.MahuaEvents
{
    /// <summary>
    /// 菜单点击事件
    /// </summary>
    public class MahuaMenuClickedMahuaEvent1
        : IMahuaMenuClickedMahuaEvent
    {
        private readonly IMahuaApi _mahuaApi;

        public MahuaMenuClickedMahuaEvent1(
            IMahuaApi mahuaApi)
        {
            _mahuaApi = mahuaApi;
        }

        public void ProcessManhuaMenuClicked(MahuaMenuClickedContext context)
        {
            if (context.Menu.Id == "menu1")
            {
                var thread = new Thread(() => new Settings.Settings().Show());
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
        }
    }
}
