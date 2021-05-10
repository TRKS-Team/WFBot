# WFBot
![GitHub Actions](https://github.com/TRKS-Team/WFBot/workflows/.NET/badge.svg)
[![GitHub release](https://img.shields.io/github/release/TRKS-Team/WFBot.svg)](https://GitHub.com/TRKS-Team/WFBot/releases/)
[![Github releases (by release)](https://img.shields.io/github/downloads/TRKS-Team/WFBot/latest/total.svg)](https://GitHub.com/TRKS-Team/WFBot/releases/)
[![License: AGPL v3](https://img.shields.io/badge/License-AGPL%20v3-blue.svg)](https://www.gnu.org/licenses/agpl-3.0)
[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2FTRKS-Team%2FWFBot.svg?type=shield)](https://app.fossa.com/projects/git%2Bgithub.com%2FTRKS-Team%2FWFBot?ref=badge_shield)  
这是一个可进行多种游戏内任务通知以及查询的 Warframe 聊天机器人.  
此项目使用 [AGPL](https://github.com/TRKS-Team/WFBot/blob/master/LICENSE) 协议授权. 如果你修改了源代码并应用到了机器人上, 请将最新的代码开源.  
我们**不允许**任何商业用途, 如果你发现有人违反, 请向我们反馈.

> 基于 莎比的 [mirai](https://github.com/mamoe/mirai) 及 [mirai-http-api](https://github.com/project-mirai/mirai-api-http) 和 [Mirai-CSharp](https://github.com/Executor-Cheng/Mirai-CSharp) 开发.  
> 翻译 云之幻整理的 [词典](https://github.com/Richasy/WFA_Lexicon)  
> 使用 云之幻 的 [WFAAPI](https://www.richasy.cn/wfa-api-overview/)  
> 任务 API 来自 [WarframeStat](https://docs.warframestat.us/)

> 本分支是 WFBot 通用版的分支, 原基于 Mahua 框架(已停更)的可在[这里](https://github.com/TRKS-Team/WFBot/tree/old-sbmahua)找到  
> 本分支将持续更新, 预计在将来支持更多种机器人平台和聊天平台, 你也可以自己适配

![MONEY](docs/images/MONEY.png) 
--by [@Lasm_Gratel](https://github.com/LasmGratel)  
~~骗钱~~赞助网址: [爱发电](https://afdian.net/@TheRealKamisama)  
您的赞助会成为我们维护此项目的动力

---

## 🚧 如何部署

[**部署指南**](docs/install.md)  
[**部署指南**](docs/install.md)  
[**部署指南**](docs/install.md)

---

## 如何使用

### 控制台命令

<details>
    <summary>自定义命令匹配的解释</summary>
比如 wiki 查询只能用 '/wiki', 我想让它支持'/维基', 使用 `PrintCommands` 找到 wiki 的命令 ID 为 `Wiki`(区分大小写), 再使用 `AddCustomCommandMatcher Wiki 维基`, 就可以使用 '/维基' 了.  
</details>

- **_添加自定义命令匹配_**: **AddCustomCommandMatcher <命令 ID> <匹配字符>**
- **_移除自定义命令匹配_**: **RemoveCustomCommandMatcher <命令 ID> <匹配字符>**
- **_列出所有命令_**: **PrintCommands**
- **_打开 UI_**: **ui**
- **_退出_**: **stop** 或 **exit**

### 群内命令

**设置内可以更改命令前需要斜杠.**

> 参数说明: <>为必填参数, []为选填参数, {}为附加选填参数, ()为补充说明

- **_遗物_**: **/遗物 <关键词>** 查询遗物的内容
- **_赤毒_**: **/赤毒** 所有赤毒任务
- **_仲裁_**: **/仲裁** 仲裁警报
- **_Wiki_**: **/wiki [关键词]** 搜索[Wiki](https://warframe.huijiwiki.com/wiki/%E9%A6%96%E9%A1%B5)上的页面
- **_午夜电波_**: **/午夜电波** 每周/每日/即将过期的挑战.
- **_机器人状态_**: **/status** 机器人的运行状态.
- **_警报_**: **/警报** 所有警报.
  > _新警报也会自动发送到**启用了通知功能的群**._
- **_入侵_**: **/入侵** 所有入侵.
  > _新入侵也会自动发送到**启用了通知功能的群**._
- **_Sentient异常事件_**: **/s船** 当前的Sentient异常事件
- **_突击_**: **/突击** 本日突击
- **_平原时间_**: **/平原** 地球&金星&火卫二的时间循环
- **_活动_**: **/活动** 所有活动
- **_虚空商人_**: **/虚空商人 (或奸商)**
  > _如果虚空商人已经抵达将会输出**所有的商品和价格**, 长度较长._
- **_WarframeMarket_**: **/查询 <物品名称> {-qr}(快捷回复) {-b}(查询买家)**
  > - _目前模糊匹配功能十分强大,无需强制按照格式_
  > - _查询未开紫卡请输入: 手枪未开紫卡_
- **_紫卡市场_**: **/紫卡 <武器名称>**
  > _数据来自 [**WFA 紫卡市场**](https://riven.richasy.cn/#/)_
- **_地球赏金_**: **/地球赏金 [赏金等级(如 5)]** 地球平原的 全部/单一 赏金任务.
- **_金星赏金_**: **/金星赏金 [赏金等级(如 5)]** 金星平原的 全部/单一 赏金任务.
- **_裂隙_**: **/裂隙** 全部裂隙.
- **_翻译_**: **/翻译 <关键词> (eg. 犀牛 prime, 致残突击)** 中 => 英 或 英 => 中
- **_小小黑_**: **/小小黑** 来查询目前追随者的信息.
  > _仅限此活动激活时可以使用_

另外还有一些不影响大局的调试命令和命令别名, 可以自己在代码中 ([私聊](https://github.com/TRKS-Team/WFBot/blob/universal/WFBot/Events/PrivateMessageReceivedEvent.cs)/[群聊](https://github.com/TRKS-Team/WFBot/blob/universal/WFBot/Events/MessageReceivedEvent.cs)) 查阅.

### **私聊**命令

请不要把七个星号(默认口令)替换为群号.

- **用于启用群通知:** `添加群 <口令> <群号>`  
  默认为: `添加群 ******* 群号`
- **用于禁用群通知:** `删除群 <口令> <群号>`  
  默认为: `删除群 ******* 群号`

### 其他东西

直接邀请机器人机器人就会同意 (可修改配置)  
默认口令: `*******` (某梗)

---

## 版权及开源库

|                            名字                             |     开源协议      |         用来干嘛         |
| :---------------------------------------------------------: | :---------------: | :----------------------: |
|     [Humanizer](https://github.com/Humanizr/Humanizer)      |        MIT        | 将时间转为中国人可读文字 |
|     [Newtonsoft.Json](https://www.newtonsoft.com/json)      |        MIT        |    比较可靠的 Json 库    |
|            [Fody](https://github.com/Fody/Fody)             |        MIT        |   GammaLibrary 一依赖    |
| [Fastenshtein](https://github.com/DanHarltey/Fastenshtein)  |        MIT        |     字符串相似度比较     |
| [TextMessageCore](https://github.com/Cyl18/TextCommandCore) |       WTFPL       |        命令分析库        |
|    [GammaLibrary](https://github.com/Cyl18/GammaLibrary)    | Anti-996 License  |        C# 工具库         |
|      [HtmlAgilityPack](https://html-agility-pack.net/)      |        MIT        |        HTML 分析         |
|       [Harmony](https://github.com/pardeike/Harmony)        |        MIT        |         插件支持         |
|    [Richasy.WFA.Sdk](https://github.com/Richasy/WFA-SDK)    | richasy Copyright |         WFA API          |
| [Mirai-CSharp](https://github.com/Executor-Cheng/Mirai-CSharp) | AGPL-3.0 | Mirai C# 连接 |
| [GitVersion](https://github.com/GitTools/GitVersion) | MIT | 提供版本号支持 |
| [Magick.NET](https://github.com/dlemstra/Magick.NET) | Apache-2.0 | 临时图片渲染 |
---

Warframe 是 Digital Extremes Ltd. 的商标.

---

## License
[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2FTRKS-Team%2FWFBot.svg?type=large)](https://app.fossa.com/projects/git%2Bgithub.com%2FTRKS-Team%2FWFBot?ref=badge_large)

## 代码部分

代码注释极少... 并且大部分都是一些瞎记的.  
我不写注释是因为: **这代码这么难写 那他应该也难读**  
如果发现 Bug 或者你有好的想法可以在[GitHub Issue](https://github.com/TRKS-Team/WFBot/issues)里直接提出, 我会尽量去修改和实现.  

苦力 [@Cyl18](https://github.com/Cyl18)

> Cyl18 是我雇佣的一个劳工  
> 他每天坐在电脑前 看到我 Commit 的时候 他就戳一下 build.bat  
> 然后将插件文件打包 上传到 GitHub 上  
> 可怜的 Cyl18 每天要做这些工作维持生活

> Cyl18 又多了好几份工作  
> 每天 24 小时他都要坐在电脑面前  
> 看着 Warframe 游戏内的信息  
> 如果有新的 他就发到群里  
> 如果群里有人调用机器人 他就手动把信息发过去

乐子: <https://github.com/TRKS-Team/WFBot/commit/a43c2c944231389b4f95489a6aa180fdb2cdf6b9#r50572036>
## 贡献者列表 [排名不分先后]

- 代码贡献:
  > **TheRealKamisama** 项目发起人 主要开发者 文档撰写 问题回答 ~~骗钱~~  
  > **Cyl18** 项目维护 代码修改 文档修改 苦力 ~~装逼~~  
  > **qiutong123** 提供了翻译功能的代码 (PR)  
  > **@wu452148993** 解决了一个令我很头疼的问题
- 问题贡献: wosiwq Aujin JJins mengpf
- 捐助者 **(不惨)**:
KONYCN
Trinitatem
爱发电用户_pPYQ
Pharsaria
LouisJane
L·A·Y
爱发电用户_Ymhw
琪露诺
wosiwq
Zatermelon
aecine
dc姐姐好棒哒
◆Philip
DreaM1ku
果汁
Flashbang233
SM_Chicov
曲水流觞℡
luoroz
玄白SAMA
难喝的鸡汤
爱发电用户_edf3
苟Cy
780712
vian
爱发电用户_Ayhf
爱发电用户_mQps
老腊肉
Neptune
君莫笑

---

感谢 JetBrain 公司为我们提供的 All-Product-License  
[![jetbrains](docs/images/jetbrains-variant-3-201x231.png)](https://www.jetbrains.com/?from=WFBot)  
**_感谢这些贡献者, 开源项目有你们才有未来_**


