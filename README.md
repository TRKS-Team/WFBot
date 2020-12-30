# WFBot

这是一个可进行多种游戏内任务通知以及查询的 Warframe 机器人.  
此项目使用 [AGPL](https://github.com/TRKS-Team/WFBot/blob/master/LICENSE) 协议授权. 如果你修改了源代码并应用到了机器人上, 请将最新的代码开源.  
我们**不允许**任何商业用途, 如果你发现有人违反, 请向我们反馈.

> 基于 莎比的 [mirai](https://github.com/mamoe/mirai) 及 [mirai-http-api](https://github.com/project-mirai/mirai-api-http) 和 [Mirai-CSharp](https://github.com/Executor-Cheng/Mirai-CSharp) 开发.  
> 翻译 [词典](https://github.com/Richasy/WFA_Lexicon) 作者: 云之幻  
> 使用 [API](https://blog.richasy.cn/document/wfa/api/) 文档由云之幻整理  
> 任务 API 来自 [WarframeStat](https://docs.warframestat.us/)

> 本分支是WFBot通用版的分支, 原基于Mahua框架(已停更)的可在[这里](https://github.com/TRKS-Team/WFBot/tree/old-sbmahua)找到  
> 本分支将持续更新, 预计在将来支持更多种机器人平台和聊天平台, 你也可以自己适配

![MONEY](docs/images/MONEY.png) --by [@Lasm_Gratel](https://github.com/LasmGratel)  
**~~骗钱~~赞助网址: [爱发电](https://afdian.net/@TheRealKamisama)**  
**您的赞助会成为我们维护此项目的动力**

## 如何部署(目前使用机器人的唯一方案)

> 如果你在部署过程中遇到了问题, 请先查看下面的 FAQ. 如果还是无法解决, 可以添加 [QQ 群](http://shang.qq.com/wpa/qunwpa?idkey=1a6da96f714791f3289ee2cafb98847efefd5c5d28e913b6bdf71b8d07e35c53)或者使用 [GitHub Issues](https://github.com/TRKS-Team/WFBot/issues).

[**部署 FAQ (常见问题解答)**](docs/faq.md)

### 第一步: 下载 mirai 机器人平台

Mirai 可以使用 ~~[MiraiOK](https://github.com/LXY1226/MiraiOK)~~ (目前 MiraiOK 不是很行 可以用 mcl) 来方便部署

> 你可以自己手写 Connector 来适配其他机器人平台(如 Discord).  
> 对其他平台的官方支持以后*会写的*  
> **我们建议将机器人运行在 24 小时运行的电脑/服务器 上.**  
> **摆脱了 ~~SB~~Mahua 框架, mirai 和 WFBot 都支持了 Linux/Windows/macOS**

### 第二步: 获取 WFBot 插件本体

#### 直接下载

1. 下载 [.NET Core 3.1](https://dotnet.microsoft.com/download/dotnet-core/3.1) 并安装. **Windows 请下载 .NET Core 3.1 Desktop Runtime, 如果不懂什么意思请加群**.
2. 先偷个懒, 加群来下, 有空再改 (目前 GitHub Release 提供的 WFBot 可用)

#### 自己编译(可选)

- 新! 针对改代码(如文字提示)又想享受自动更新的客户 你可以写一个 WFBot 的[插件](docs/plugin.md)  
↑ 自动更新亡了 以后写

> 如果你不需要修改代码, 我们强烈建议你从上面下载.  
> 如果你修改了代码并应用到机器人上, 请在 GitHub 上开源其最新版本.  
> **如果你使用非官方版 我们将不保证运行安全与稳定.**

1. 安装 `Visual Studio 2019` , 以及 `.NET Core 3.1 SDK.`
2. 下载这个仓库, 使用`Visual Studio 2019`打开`TRKS.WF.QQBot\WFBot.sln`
3. 找到工具栏下的解决方案配置 选择`Linux Release`或`Windows Release`  
4. 在上方工具栏选择`生成-生成解决方案` 左下角应该出现`生成成功`
5. 将`TRKS.WF.QQBot\MiraiHTTPConnector\bin\Windows Release\netcoreapp3.1`所有文件拖入`TRKS.WF.QQBot\WFBot\bin\Windows Release\netcoreapp3.1\WFBotConnector`
6. 将`WFBot\bin\Windows Release\netcoreapp3.1`打包放到合适的位置

*这堆东西应该以后会有一个脚本完成*

### 第三步: 安装 mirai 插件和修改配置文件

本插件使用了 mirai-api-http 插件 你可以在[Github Release](https://github.com/project-mirai/mirai-api-http/releases/latest)找到最新版  
运行一遍 mirai, 并将下载的 .jar 文件拖入`plugins`文件夹内  
再次运行 mirai, 修改`config\MiraiApiHttp\setting.yml`  
我们建议仅修改`authKey`字段后双引号内的内容, 设置一个八位数以上的密码  
运行一次 WFBot, 找到目录下的`MiraiConfig.json`  
在`AuthKey`后引号内填入刚才设置的密码, 将`BotQQ`后数字改为机器人的 QQ 号  
运行 mirai, 再运行 WFBot, 你应该能 在 WFBot 控制台看到`WFBot fully loaded.`  
这个时候你可以输入 `ui` 并按下 Enter 键来启动配置界面
### 第四步: 自定义

> WFBot 控制台内输入 ui 可以打开设置窗口(仅Windows)(以后会适配全平台)   

可自定义的内容如下:

- 修改群通知功能所用的口令 **(默认为 7 个 \*)**
- 是否需要前导`/`来使用命令 **(默认需要)**
- 包含 哪些奖励的入侵任务 需要通知到群内 **(默认参见设置)**
- 用于管理机器人的 QQ号 **(填你自己的, 用来修改敏感信息和接收报错)**
- 是否自动同意 别人邀请机器人入群 **(无需群内管理)** 和 自主申请入群 **(需群内管理)**
- WFA 授权的 `ClientId` 和 `ClientSecret` (非必须, 见下)
- WM商品和紫卡查询单次发送的条数
- 每分钟机器人调用次数限制
- 是否使用中转后的 WarframeMarket 接口 (需 WFA 授权)
- [GithubToken](docs/token.md) **(非必须)**

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
> 参数说明: <>为必填参数, []为选填参数, {}为附加选填参数, ()为补充说明

- **_赤毒_**: **/赤毒** 所有赤毒任务
- **_仲裁_**: **/仲裁** 仲裁警报
- **_Wiki_**: **/wiki [关键词]** 搜索[Wiki](https://warframe.huijiwiki.com/wiki/%E9%A6%96%E9%A1%B5)上的页面
- **_午夜电波_**: **/午夜电波** 每周/每日/即将过期的挑战.
- **_机器人状态_**: **/status** 机器人的运行状态.
- **_警报_**: **/警报** 所有警报.
  > _新警报也会自动发送到**启用了通知功能的群**._
- **_入侵_**: **/入侵** 所有入侵.
  > _新入侵也会自动发送到**启用了通知功能的群**._
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
- **_地球赏金_**: **/地球赏金 [赏金等级(如5)]** 地球平原的 全部/单一 赏金任务.
- **_金星赏金_**: **/金星赏金 [赏金等级(如5)]** 金星平原的 全部/单一 赏金任务.
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
  > **@wu452148993** 解决了一个令我很头疼的问题
- 问题贡献: wosiwq Aujin JJins mengpf
- 捐助者 **(不惨)**:
 KONYCN
Trinitatem
爱发电用户_pPYQ
Pharsaria
LouisJane
L·A·Y
dc姐姐好棒哒
爱发电用户_Ymhw
琪露诺
wosiwq
Zatermelon
aecine
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
Deadwings
爱发电用户_x9FU
不够温柔
爱发电用户_FtaS
大番茄一号
爱发电用户_JV6j

感谢 JetBrain 公司为我们提供的 All-Product-License  
[![jetbrains](docs/images/jetbrains-variant-3-201x231.png)](https://www.jetbrains.com/?from=WFBot)  
**_感谢这些贡献者, 开源项目有你们才有未来_**
