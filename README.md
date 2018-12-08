# WFBot
这是一个可进行多种游戏内任务通知以及查询的 Warframe 机器人  
基于 [Newbe.Mahua.Framework](https://github.com/newbe36524/Newbe.Mahua.Framework) 开发  
可部署于多种QQ机器人框架上  

翻译[词典](https://github.com/Richasy/WFA_Lexicon)作者: 云之幻  
使用的 [API](https://blog.richasy.cn/document/wfa/api/) 文档由云之幻整理   
任务查询使用 [API](https://docs.warframestat.us/) 来自 WarframeStat  

## 部署方法  
目前有一个由我维护, 稳定运行的机器人, 可直接邀请入群 QQ: 2504645673   
个人用户可添加 QQ 群: 938873897

----------

自己部署: 请选择以下几种QQ机器人平台:  
**部分机器人平台 (如酷Q) 需要启用*开发者模式*才能看到本插件 (感谢来自一位用户的提醒), 具体启用方法请自行查找.**

- [酷Q Pro/Air](https://cqp.cc/)(拥有免费阉割版)**推荐**  
- [QQLight](https://www.52chat.cc/download.php)(付费)  
- [Cleverqq(原IRQQ)](https://www.cleverqq.cn/)(拥有免费阉割版)  
- [MyPCQQ](https://mypcqq.cc/)(付费)  

### 安装插件文件
#### 从 GitHub Release
1. 从[这里](https://github.com/TRKS-Team/WFBot/releases/latest)选择对应的平台并下载, 解压到机器人根目录.

#### 自己编译
1. 安装 `Visual Studio 2017`, 以及 `.NET Framework 4.6.2 SDK.`  
2. Clone 这个仓库, 并使用`TRKS.WF.QQBot\build.bat`来生成一份插件.  
3. 将生成的所有`TRKS.WF.QQBot\bin\{对应QQ平台}`文件夹内的所有文件拖入机器人根目录.  
---
> 提示: 如果机器人启动就报错 可尝试删除`YUELUO\TRKS.WF.QQBot\TRKS.WF.QQBot.dll.config`   
  

之后可以在插件设置内修改入侵提醒物品以及授权口令.
可修改的内容如下: 
- 修改群通知功能所用的口令
- 哪些入侵任务的奖励需要通知群内
- 用于接收报错的 QQ 号 **(调试使用, 建议留空)**
- 是否自动同意别人邀请入群 **(无需群内管理)** 和申请入群 **(需群内管理)**   
- 是否对所有 **启用了通知功能** 的群发送一条通知

---
## 如何使用
### 群内命令
- **警报**: 可使用 **/警报** 来查询当前的所有警报.   
     >*新警报也会自动发送到**启用了通知功能的群**.*  
- **入侵**: 可使用 **/入侵** 来查询当前的所有入侵.   
    > *新入侵也会自动发送到**启用了通知功能的群**.*  
- **突击**: 可使用 **/突击** 来查询当前的所有突击.   
    > *突击的奖励池为一般奖励池.*  
- **平原时间**: 可使用 **/平原** 来查询 **地球平原** 现在的时间 和 **奥布山谷 (金星平原)** 现在的温度.  
虚空商人信息 可使用 **/虚空商人 (或者你输入奸商也可以)** 来查询奸商的状态.  
  > *如果虚空商人已经抵达将会输出**所有的商品和价格**, 长度较长.*  
- **WarframeMarket** 可使用 **/查询  \[物品名称]**
    > - *物品名不区分大小写, 无需空格*
    > - *物品名**必须**标准*  
    > - *查询一个物品需要后面加一套*   
    > - *查询 `prime` 版物品必须加 `prime` 后缀*  
    > - *`prime` 不可以缩写成 `p`*  
    > - *查询未开紫卡请输入: 手枪未开紫卡*  
- **赏金**: 可使用 **/赏金(或同义词) \[赏金数] (eg. 赏金一就是 1)** 来查询**地球**和**平原**的单一赏金任务.  
    > *必须输入需要第几个赏金.*  
- **裂隙**: 可使用 **/裂隙  \[关键词] (eg. 前纪, 歼灭)** 来查询所有和关键词有关的裂隙.  

### **私聊**命令
请不要把七个星号(默认口令)替换为群号.    
- **用于启用群通知:** `添加群 [口令] [群号]`  
默认为: `添加群 ******* 群号`  
- **用于禁用群通知:** `删除群 [口令] [群号]`  
默认为: `删除群 ******* 群号`   

### 其他内容
直接邀请机器人机器人就会同意. (可修改配置)  
默认口令: ******* (对没错就是七个星号,有人猜得出我玩了什么梗吗)

----------
## 使用的开源库
- [Autofac](https://github.com/autofac/Autofac) (MIT)
- [Humanizer](https://github.com/Humanizr/Humanizer) (MIT)
- [MediatR](https://github.com/jbogard/MediatR) (Apache-2.0)
- [MessagePack](https://github.com/neuecc/MessagePack-CSharp/) (MIT)
- [Newbe.Mahua](http://www.newbe.pro/) (MIT)
- [Newtonsoft.Json](https://www.newtonsoft.com/json) (MIT)
- [Refit](https://github.com/reactiveui/refit) (MIT)
- [Costura](https://github.com/Fody/Costura) (MIT)
- [Fody](https://github.com/Fody/Fody) (MIT)
----------
## 代码部分
代码注释极少... 并且大部分都是一些瞎记的.  
所以如果有 dalao 需要的话可以单独找 我来询问...  
如果有 bug 或者你有好的想法可以作为 feature 可以在 Issue 里直接提出,我会尽量去修改和实现.  
特别鸣谢[@Cyl18](https://github.com/Cyl18)  