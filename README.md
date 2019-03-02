# WFBot

![](https://ci.appveyor.com/api/projects/status/xdrcdmge2pub0ga6)  
这是一个可进行多种游戏内任务通知以及查询的 Warframe 机器人.  
基于 [Newbe.Mahua.Framework](https://github.com/newbe36524/Newbe.Mahua.Framework) 开发.

- 翻译 [词典](https://github.com/Richasy/WFA_Lexicon) 作者: 云之幻
- 使用 [API](https://blog.richasy.cn/document/wfa/api/) 文档由云之幻整理
- 任务 [API](https://docs.warframestat.us/) 来自 WarframeStat

![MONEY](docs/images/MONEY.png) --by @Lasm_Gratel  
**~~骗钱~~赞助网址: [爱发电](https://afdian.net/@TheRealKamisama)**  
**或者你点几次[链接](http://lopoteam.com/G6U)也能支持到我**  
**您的赞助...可能会让机器人变得更好吧..?**

> 本插件附带自动更新.  
> 最新的插件 会通过 自动更新 部署到 自动更新版本的机器人 上, 如果我手滑引爆了所有机器人, 请不要慌张.  
> 如果真的全爆了, **没关系** 因为我的机器人也爆了.  
> 很快 [Github Release](https://github.com/TRKS-Team/WFBot/releases/latest) 上将会上传正确的版本 **手动更新**即可.  
> 此处记录引爆次数: \*3

> 目前有一个由我维护, 暂时稳定的机器人, 现在可以直接邀请入群 QQ: `2504645673`  
> 个人用户可添加 QQ 群: [938873897](http://shang.qq.com/wpa/qunwpa?idkey=0171441db21449373d1070bdcb5b26d272131bd3579f7816586446c0bbb8ec12)  
> 目前**不建议**使用我部署的机器人! 负荷过大导致**经常失效**! 处在后面位置的群收到通知的速度会很慢!  
> **自己按照下面的说明部署可以享受完全体**

## 如何部署

> 如果你在部署过程中遇到了问题, 请先查看下面的 FAQ. 如果还是无法解决, 可以添加上面的个人 QQ 群或者使用 [GitHub Issues](https://github.com/TRKS-Team/WFBot/issues).

[**部署 FAQ (常见问题解答)**](docs/faq.md)

### 第一步: 选择以下几种 QQ 机器人平台

- [酷 Q Pro / Air](https://cqp.cc/) - 拥有免费阉割版 **需要开启开发者模式** 推荐
- [QQLight](https://www.52chat.cc/download.php) - 付费
- [Cleverqq(原 IRQQ)](https://www.cleverqq.cn/) - 拥有免费阉割版
- [MyPCQQ](https://mypcqq.cc/) - 付费

> **酷 Q 平台 需要启用*开发者模式*才能看到本插件 (感谢来自一位用户的提醒), 具体启用方法参见 [FAQ](docs/faq.md).**  
> **我们建议你将你选择的机器人放在一台 24 小时运行的电脑上.**

### 第二步: 安装插件文件

#### 从 GitHub Release / AppVeyor (推荐)

1. 下载 [.NET Framework 4.6.2](https://dotnet.microsoft.com/download/thank-you/net462) 并安装. (Windows 10 最新版自带)
2. 从 [Github Release](https://github.com/TRKS-Team/WFBot/releases/latest) / [AppVeyor](https://ci.appveyor.com/project/TRKS-Team/wfbot/build/artifacts) 选择对应的平台并下载, 解压到机器人根目录.
3. 享受这个插~~♂~~件的的快感吧!

#### 或者: 自己编译 (特殊需求) (不推荐)

> 如果你不需要修改代码, 我们强烈建议你从上面下载.  
> 我们强烈建议在 Windows 平台下编译这个项目.

1. 安装 `Visual Studio 2017 / 2019`, 以及 `.NET Framework 4.6.2 SDK.`
2. 下载这个仓库, 使用 `TRKS.WF.QQBot\build.bat` 来生成一份插件.

   ```bash
    git clone https://github.com/TRKS-Team/WFBot.git
    ./WFBot/TRKS.WF.QQBot/build.bat
   ```

3. 将生成的所有 `TRKS.WF.QQBot\bin\[对应 QQ 平台]` 文件夹内的所有文件拖入机器人根目录.

### 第三步: 自定义

可以在插件设置内干一些奇怪的事情.  
可自定义的内容如下:

- 修改群通知功能所用的口令 **(默认为 7 个 \*)**
- 是否需要前导`/`来使用命令 **(默认需要)**
- 包含 哪些奖励的入侵任务 需要通知到群内 **(默认参见设置)**
- 用于接收报错的 QQ 号 **(调试使用, 建议留空)**
- 是否自动同意 别人邀请机器人入群 **(无需群内管理)** 和 自主申请入群 **(需群内管理)**
- WFA 授权的 `ClientId` 和 `ClientSecret` (非必须, 见下)
- 是否使用第三方词库 和 中转后的 WarframeMarket 接口 (需 WFA 授权)

可以使用的功能如下:

- 对所有 **启用了通知功能** 的群发送一条通知

### 启用 WFA 授权 **(非必须)**

设置内填入从云之幻处授权的 `ClientId` 和 `ClientSecret` 即可启用  
**如果你不知道这俩是干嘛的, 就别瞎填, 因为我的用户创造力都好强啊**

> **不启用授权不影响基本功能**

> **WM 查询** 可使用中转过后的服务器 **速度大概更高**  
> **紫卡市场** 使用 **必须** 启用 WFA 授权

**授权获取** 请查看 **[云之幻的 API 文档](https://blog.richasy.cn/document/wfa/api/)**

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
- **活动**: 可使用 **/活动** 来查看目前的所有活动
- **虚空商人信息**: 可使用 **/虚空商人 (或奸商)** 来查询奸商的状态.
  > _如果虚空商人已经抵达将会输出**所有的商品和价格**, 长度较长._
- **WarframeMarket** 可使用 **/查询 \[物品名称]**
  > - _目前模糊匹配功能十分强大,无需强制按照格式_
  > - _查询未开紫卡请输入: 手枪未开紫卡_
- **紫卡市场**: 可使用 **/紫卡 \[武器名称]**
  > _数据来自 [**WFA 紫卡市场**](https://riven.richasy.cn/#/)_
- **地球赏金**: 可使用 **/地球赏金 \[第几个(可选)]** 来查询地球平原的 全部/单一 赏金任务.
- **金星赏金**: 可使用 **/金星赏金 \[第几个(可选)]** 来查询金星平原的 全部/单一 赏金任务.
- **裂隙**: 可使用 **/裂隙** 来查询全部裂隙.
  > _目前不需要输入任何关键词了._
- **遗物**: 可使用 **/遗物 \[关键词] (eg. 后纪 s3, 前纪 B3)** 来查询所有与关键词有关的遗物.
- **翻译**: 可使用 **/遗物 \[关键词] (eg. 犀牛 prime, 致残突击)** 来进行 中 => 英 或 英 => 中
  > _关键词务必标准_
- **小小黑**: 可使用 **/小小黑** 来查询目前追随者的信息.
  > _仅限此活动激活时可以使用_

另外还有一些不影响大局的调试命令和命令别名, 可以自己在代码中 ([私聊](https://github.com/TRKS-Team/WFBot/blob/master/TRKS.WF.QQBot/MahuaEvents/PrivateMessageReceivedMahuaEvent2.cs#L68)/[群聊](https://github.com/TRKS-Team/WFBot/blob/master/TRKS.WF.QQBot/MahuaEvents/GroupMessageReceivedMahuaEvent1.cs#L53)) 查阅.

### **私聊**命令

请不要把七个星号(默认口令)替换为群号.

- **用于启用群通知:** `添加群 [口令] [群号]`  
  默认为: `添加群 ******* 群号`
- **用于禁用群通知:** `删除群 [口令] [群号]`  
  默认为: `删除群 ******* 群号`

### 其他东西

直接邀请机器人机器人就会同意. (可修改配置)  
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
| [TextMessageCore](https://github.com/Cyl18/TextCommandCore)  |   WTFPL    |   我写的我写的!命令库    |

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

> Cyl18 又多了好几份工作  
> 每天 24 小时他都要坐在电脑面前  
> 看着 Warframe 游戏内的信息  
> 如果有新的 他就发到群里  
> 如果群里有人调用机器人 他就手动把信息发过去

## 贡献者列表 [排名不分先后]

- 代码贡献:
  > **TheRealKamisama** 项目发起人 主要开发者 文档撰写 问题回答 ~~骗钱~~  
  > **Cyl18** 项目维护 代码修改 文档修改 苦力 ~~装逼~~  
  > **qiutong123** 提供了翻译功能的代码 (PR)
- 问题贡献: wosiwq Aujin JJins mengpf
- 捐助者 **(真惨)**: KONYCN Trinitatem wosiwq Cyl18 爱发电用户\_Ymhw
