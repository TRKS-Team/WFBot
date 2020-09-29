# WFBot

这是一个可进行多种游戏内任务通知以及查询的 Warframe 机器人.  
此项目使用 [AGPL](https://github.com/TRKS-Team/WFBot/blob/master/LICENSE) 协议授权. 如果你修改了源代码并应用到了机器人上, 请将最新的代码开源.  
我们**不允许**任何商业用途, 如果你发现有人违反, 请向我们反馈.

> 基于 漂亮的 [Mirai](https://github.com/mamoe/mirai) 开发.  
> 翻译 [词典](https://github.com/Richasy/WFA_Lexicon) 作者: 云之幻  
> 使用 [API](https://blog.richasy.cn/document/wfa/api/) 文档由云之幻整理  
> 任务 API 来自 [WarframeStat](https://docs.warframestat.us/)

![MONEY](docs/images/MONEY.png) --by @Lasm_Gratel  
**~~骗钱~~赞助网址: [爱发电](https://afdian.net/@TheRealKamisama)**  
**您的赞助当然~~你妈~~会用来维持公用机器人, 也能推动我继续维护本插件.**

→[思维导图](https://www.processon.com/view/link/5d1e2622e4b0fdb331d33d23#map)←

![Processon](docs/images/processon.png)

## 如何部署(目前使用机器人的唯一方案)

> 如果你在部署过程中遇到了问题, 请先查看下面的 FAQ. 如果还是无法解决, 可以添加上面的个人 QQ 群或者使用 [GitHub Issues](https://github.com/TRKS-Team/WFBot/issues).

[**部署 FAQ (常见问题解答)**](docs/faq.md)

### 第一步: 选择以下一种 QQ 机器人平台

Mirai 可以使用 [MiraiOK](https://github.com/LXY1226/MiraiOK) 来方便部署

> 你可以自己手写 Connector 来适配新的 QQ 机器人.  
> **我们建议你将你选择的机器人放在一台 24 小时运行的电脑/服务器上.**

### 第二步: 安装插件文件

#### 直接下载

1. 下载 [~~.NET Core 3.1~~](https://dotnet.microsoft.com/download/dotnet-core/3.1) 并安装. **Windows 请下载 Desktop Runtime, 如果不懂什么意思请加群**.
2. 先偷个懒, 加群来下, 有空再改

#### 或者: 自己编译

- 新! 针对改代码(如文字提示)又想~~享受~~自动更新的客户 你可以写一个 WFBot 的[插件](docs/plugin.md)

> 如果你不需要修改代码, 我们强烈建议你从上面下载.  
> 如果你修改了代码并应用到机器人上, 请在 GitHub 上开源其最新版本.  
> **如果你使用非官方版 我们将不会解答除了代码结构和原理之外的其他问题.**

1. 安装 `Visual Studio 2019` , 以及 `.NET Core 3.1 SDK.`
2. 下载这个仓库, 右键 WFBot 点部署, 有 Linux Deploy 和 Windows Deploy 两个版本.

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

- **_赤毒_**: **/赤毒** 当前的所有赤毒任务
- **_仲裁_**: **/仲裁** 当前的仲裁警报
- **_Wiki_**: **/wiki [关键词]** 搜索[Wiki](https://warframe.huijiwiki.com/wiki/%E9%A6%96%E9%A1%B5)上的页面
- **_午夜电波_**: **/午夜电波** 每周/每日/即将过期 的挑战.
- **_机器人状态_**: **/status** 机器人的运行状态.
- **_警报_**: **/警报** 当前的所有警报.
  > _新警报也会自动发送到**启用了通知功能的群**._
- **_入侵_**: **/入侵** 当前的所有入侵.
  > _新入侵也会自动发送到**启用了通知功能的群**._
- **_S 船_**: **/s 船** 来查询 s 船的信息
  > _新 S 船也会自动发送到**启用了通知功能的群**._
- **_突击_**: **/突击** 当前的所有突击.
- **_平原时间_**: **/平原** **地球平原** 现在的时间 和 **奥布山谷 (金星平原)** 现在的温度.
- **_活动_**: **/活动** 目前的所有活动
- **_虚空商人_**: **/虚空商人 (或奸商)** 奸商的状态.
  > _如果虚空商人已经抵达将会输出**所有的商品和价格**, 长度较长._
- **_WarframeMarket_**: **/查询 \[物品名称]**
  > - _目前模糊匹配功能十分强大,无需强制按照格式_
  > - _查询未开紫卡请输入: 手枪未开紫卡_
- **_紫卡市场_**: **/紫卡 \[武器名称]**
  > _数据来自 [**WFA 紫卡市场**](https://riven.richasy.cn/#/)_
- **_地球赏金_**: **/地球赏金 \[第几个(可选)]** 地球平原的 全部/单一 赏金任务.
- **_金星赏金_**: **/金星赏金 \[第几个(可选)]** 金星平原的 全部/单一 赏金任务.
- **_裂隙_**: **/裂隙** 全部裂隙.
- **_遗物_**: **/遗物 \[关键词] (eg. 后纪 s3, 前纪 B3)** 所有与关键词有关的遗物.
- **_翻译_**: **/遗物 \[关键词] (eg. 犀牛 prime, 致残突击)** 中 => 英 或 英 => 中
- **_小小黑_**: **/小小黑** 来查询目前追随者的信息.
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

## 版权及开源库

|                            名字                             | 开源协议 |         用来干嘛         |
| :---------------------------------------------------------: | :------: | :----------------------: |
|     [Humanizer](https://github.com/Humanizr/Humanizer)      |   MIT    | 将时间转为中国人可读文字 |
|     [Newtonsoft.Json](https://www.newtonsoft.com/json)      |   MIT    |    比较可靠的 Json 库    |
|         [Costura](https://github.com/Fody/Costura)          |   MIT    |       集成引用文件       |
|            [Fody](https://github.com/Fody/Fody)             |   MIT    |       Costura 依赖       |
| [Fastenshtein](https://github.com/DanHarltey/Fastenshtein)  |   MIT    |     字符串相似度比较     |
| [TextMessageCore](https://github.com/Cyl18/TextCommandCore) |  WTFPL   |   我写的我写的!命令库    |

Warframe 是 Digital Extremes Ltd. 的商标.

---

## 代码部分

代码注释极少... 并且大部分都是一些瞎记的.  
我不写注释是因为: **这代码这么难写 那他应该也难读**  
如果发现 Bug 或者你有好的想法可以在[GitHub Issue](https://github.com/TRKS-Team/WFBot/issues)里直接提出, 我会尽量去修改和实现.  
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
  > **[@wu452148993](https://github.com/wu452148993)** 解决了一个令我很头疼的问题
- 问题贡献: wosiwq Aujin JJins mengpf
- 捐助者 **(真惨)**:
  KONYCN
  Trinitatem
  Pharsaria
  爱发电用户\_pPYQ
  L·A·Y
  aecine
  Zatermelon
  wosiwq
  琪露诺
  爱发电用户\_Ymhw
  难喝的鸡汤
  玄白 SAMA
  luoroz
  曲水流觞 ℡
  SM_Chicov
  Flashbang233
  果汁
  DreaM1ku
  780712
  苟 Cy
  爱发电用户\_JV6j
  大番茄一号
  爱发电用户\_FtaS
  不够温柔
  爱发电用户\_x9FU
  Deadwings
  君莫笑
  Neptune
  老腊肉
  爱发电用户\_mQps

**_感谢这些赞助者, 开源项目有你们才有未来_**
