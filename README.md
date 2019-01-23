# WFBot

![](https://ci.appveyor.com/api/projects/status/xdrcdmge2pub0ga6)  
这是一个可进行多种游戏内任务通知以及查询的 Warframe 机器人  
基于 [Newbe.Mahua.Framework](https://github.com/newbe36524/Newbe.Mahua.Framework) 开发

翻译 [词典](https://github.com/Richasy/WFA_Lexicon) 作者: 云之幻  
使用 [API](https://blog.richasy.cn/document/wfa/api/) 文档由云之幻整理  
任务 [API](https://docs.warframestat.us/) 来自 WarframeStat

**~~骗钱~~赞助网址: [爱发电](https://afdian.net/@TheRealKamisama)**  
您的赞助...可能会让机器人变得更好吧
> 自动更新将会把最新的插件部署到所有机器人上, 如果我手滑引爆了所有机器人, 请不要慌张.  
> 如果真的全爆了 没关系 因为我的机器人也爆了.  
> 很快[Github Release](https://github.com/TRKS-Team/WFBot/releases/latest)上将会上传正确的版本 **手动更新**即可.  
> 此处记录引爆次数: *2

> 目前有一个由我维护, ~~稳定运行~~ ~~现在已经不稳定~~ 暂时稳定的机器人, ~~可直接邀请入群~~ 现在拉不进任何群 QQ: `2504645673`  
> 个人用户可添加 QQ 群: `938873897`  
> 目前**不建议**使用我部署的机器人! 负荷过大导致**经常失效**!  
> **自己按照下面的说明部署可以享受完全体**

## 如何部署

> **_[部署 FAQ (常见问题)](docs/faq.md)_**

> 如果你在部署过程中遇到了问题, 请先查看上面的 FAQ. 如果还是无法解决, 可以添加上面的个人 QQ 群或者使用 [GitHub Issues](https://github.com/TRKS-Team/WFBot/issues).

### 第一步: 选择以下几种 QQ 机器人平台

- [酷 Q Pro / Air](https://cqp.cc/) - 拥有免费阉割版 推荐
- [QQLight](https://www.52chat.cc/download.php) - 付费
- [Cleverqq(原 IRQQ)](https://www.cleverqq.cn/) - 拥有免费阉割版
- [MyPCQQ](https://mypcqq.cc/) - 付费

> **部分机器人平台 (如酷 Q) 需要启用*开发者模式*才能看到本插件 (感谢来自一位用户的提醒), 具体启用方法请自行查找.**

> **请将你选择的机器人放在一台 24 小时运行的电脑上.**

### 第二步: 安装插件文件

#### 从 GitHub Release / AppVeyor (推荐)

1. 下载 [.NET Framework 4.6.2](https://dotnet.microsoft.com/download/thank-you/net462) 并安装.
2. 从 [Github Release](https://github.com/TRKS-Team/WFBot/releases/latest) / [AppVeyor](https://ci.appveyor.com/project/TRKS-Team/wfbot/build/artifacts) 选择对应的平台并下载, 解压到机器人根目录.

#### 自己编译

1. 安装 `Visual Studio 2017/2019`, 以及 `.NET Framework 4.6.2 SDK.`
2. Clone 这个仓库, 并使用`TRKS.WF.QQBot\build.bat`来生成一份插件.
3. 将生成的所有`TRKS.WF.QQBot\bin\[对应 QQ 平台]`文件夹内的所有文件拖入机器人根目录.

---

> 提示: 由于某些玄学问题可能导致报错, 请在自己编译后请删除`YUELUO\TRKS.WF.QQBot\TRKS.WF.QQBot.dll.config`. 从 GitHub Release 下载不受此影响.

### 第三步: 自定义

需要在插件设置内修改一些奇怪的东西.  
需修改的内容如下:

- 修改群通知功能所用的口令 **(不修改将无法启用通知功能)**
- 哪些入侵任务的奖励需要通知群内
- 用于接收报错的 QQ 号 **(调试使用, 建议留空)**
- 是否自动同意别人邀请入群 **(无需群内管理)** 和申请入群 **(需群内管理)**
- 是否对所有 **启用了通知功能** 的群发送一条通知
- WFA授权的ClientId和ClientSecret  

#### 启用WFA授权

设置内填入ClientId和ClientSecret即可启用  
**WM查询** 将使用中转过后的服务器 **速度更高**  
**紫卡市场** 的使用必须启用WFA授权  
授权获取查看[API文档](https://blog.richasy.cn/document/wfa/api/)

---

## 如何使用

### 群内命令

**设置内可以更改命令前无需斜杠.**

- **警报**: 可使用 **/警报** 来查询当前的所有警报.
  > _新警报也会自动发送到**启用了通知功能的群**._
- **入侵**: 可使用 **/入侵** 来查询当前的所有入侵.
  > _新入侵也会自动发送到**启用了通知功能的群**._
- **突击**: 可使用 **/突击** 来查询当前的所有突击.
  > _突击的奖励池为一般奖励池._
- **平原时间**: 可使用 **/平原** 来查询 **地球平原** 现在的时间 和 **奥布山谷 (金星平原)** 现在的温度.
- **活动**:可使用 **/活动** 来查看目前的所有活动
- **虚空商人信息**: 可使用 **/虚空商人 (或奸商)** 来查询奸商的状态.
  > _如果虚空商人已经抵达将会输出**所有的商品和价格**, 长度较长._
- **WarframeMarket** 可使用 **/查询 \[物品名称]**
  > - _目前模糊匹配功能十分强大,无需强制按照格式_
  > - _查询未开紫卡请输入: 手枪未开紫卡_
- **紫卡市场**: 可使用 **/紫卡 \[武器名称]**
  > _数据来自 [**WFA 紫卡市场**](https://riven.richasy.cn/#/)_
- **地球赏金**: 可使用 **/地球赏金\[数字(可无)]** 来查询地球平原的 全部//单一 赏金任务.  
- **金星赏金**: 可使用 **/金星赏金 \[数字(可无)]** 来查询金星平原的 全部//单一 赏金任务.  
- **裂隙**: 可使用 **/裂隙** 来查询全部裂隙.
  >_目前不需要输入任何关键词了._
- **遗物**: 可使用 **/遗物 \[关键词] (eg. 后纪 s3, 前纪 B3)** 来查询所有与关键词有关的遗物.
- **翻译**: 可使用 **/遗物 \[关键词] (eg. 犀牛prime, 致残突击)** 来进行 中 => 英 或 英 => 中
  >_关键词务必标准_

### **私聊**命令

请不要把七个星号(默认口令)替换为群号.

- **用于启用群通知:** `添加群 [口令] [群号]`  
  默认为: `添加群 ******* 群号`
- **用于禁用群通知:** `删除群 [口令] [群号]`  
  默认为: `删除群 ******* 群号`

### 其他东西

直接邀请机器人机器人就会同意. (可修改配置)(官方机器人已经无法邀请 请加个人用户群申请.)  
默认口令: `*******` (对没错就是七个星号, 有人猜得出我玩了什么梗吗)

---

## 用到的开源库

|                             名字                             |  开源协议  |         用来干嘛         |
| :----------------------------------------------------------: | :--------: | :----------------------: |
|        [Autofac](https://github.com/autofac/Autofac)         |    MIT     |        Mahua 依赖        |
|        [MediatR](https://github.com/jbogard/MediatR)         | Apache-2.0 |        Mahua 依赖        |
| [MessagePack](https://github.com/neuecc/MessagePack-CSharp/) |    MIT     |        Mahua 依赖        |
|         [Refit](https://github.com/reactiveui/refit)         |    MIT     |        Mahua 依赖        |
|             [Newbe.Mahua](http://www.newbe.pro/)             |    MIT     |      QQ 机器人框架       |
|      [Humanizer](https://github.com/Humanizr/Humanizer)      |    MIT     | 将时间转为中国人可读文字 |
|      [Newtonsoft.Json](https://www.newtonsoft.com/json)      |    MIT     |    比较可靠的 Json 库    |
|          [Costura](https://github.com/Fody/Costura)          |    MIT     |       集成引用文件       |
|             [Fody](https://github.com/Fody/Fody)             |    MIT     |       Costura 依赖       |
|  [Fastenshtein](https://github.com/DanHarltey/Fastenshtein)  |    MIT     |     字符串相似度比较     |

---

## 代码部分

代码注释极少... 并且大部分都是一些瞎记的.  
所以如果有 dalao 需要的话可以单独找我...  
如果有 bug 或者你有好的想法作为 Feature 可以在[Github Issue](https://github.com/TRKS-Team/WFBot/issues)里直接提出, 我会尽量去修改和实现.  
特别鸣谢 [@Cyl18](https://github.com/Cyl18)  
> Cyl18 是我雇佣的一个劳工  
> 他每天坐在电脑前 看到我 Commit 的时候 他就戳一下 build.bat  
> 然后将插件文件打包 上传到 GitHub 上  
> 可怜的 Cyl18 每天要做这些工作维持生活  
