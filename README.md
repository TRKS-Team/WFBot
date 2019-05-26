# WFBot

![](https://ci.appveyor.com/api/projects/status/xdrcdmge2pub0ga6)  
这是一个可进行多种游戏内任务通知以及查询的 Warframe 机器人.  
此项目使用 [AGPL](https://github.com/TRKS-Team/WFBot/blob/master/LICENSE) 协议授权. 如果你修改了源代码并应用到了机器人上, 请将最新的代码开源.  
我们**不允许**任何商业用途, 如果你发现有人违反, 请向我反馈.  
基于 [Newbe.Mahua.Framework](https://github.com/newbe36524/Newbe.Mahua.Framework) 开发.

- 翻译 [词典](https://github.com/Richasy/WFA_Lexicon) 作者: 云之幻
- 使用 [API](https://blog.richasy.cn/document/wfa/api/) 文档由云之幻整理
- 任务 API 来自 [WarframeStat](https://docs.warframestat.us/)

![MONEY](docs/images/MONEY.png) --by @Lasm_Gratel  
**~~骗钱~~赞助网址: [爱发电](https://afdian.net/@TheRealKamisama)**  
**您的赞助会用来维持公用机器人,也能推动我继续维护本插件.**

> 本插件附带自动更新.  
> 最新的插件 会通过 自动更新 部署到 自动更新版本的机器人 上, 如果我手滑引爆了所有机器人, 请不要慌张.  
> 如果真的全爆了, **没关系** 因为我的机器人也爆了.  
> 很快 [GitHub Release](https://github.com/TRKS-Team/WFBot/releases/latest) 上将会上传正确的版本 **手动更新**即可.  
> 此处记录引爆次数: \*4

> ~~目前有一个由我维护, 暂时稳定的机器人, 现在可以直接邀请入群 QQ: `2504645673`~~  
> **↑ 此公用机器人已经被永久封号,近期可能不会恢复了...有需求的请自行部署.**  
> 用户请务必添加 QQ 群: [878527767](http://shang.qq.com/wpa/qunwpa?idkey=1a6da96f714791f3289ee2cafb98847efefd5c5d28e913b6bdf71b8d07e35c53)  
> ~~目前**不建议**使用我部署的机器人! 负荷过大导致**经常失效**! 处在后面位置的群收到通知的速度会很慢!~~  
> **自己按照下面的说明部署可以享受完全体**

## 如何部署

> 如果你在部署过程中遇到了问题, 请先查看下面的 FAQ. 如果还是无法解决, 可以添加上面的个人 QQ 群或者使用 [GitHub Issues](https://github.com/TRKS-Team/WFBot/issues).

[**部署 FAQ (常见问题解答)**](docs/faq.md)

### 第一步: 选择以下几种 QQ 机器人平台

- [酷 Q Pro / Air](https://cqp.cc/) - 拥有免费阉割版 **需要开启开发者模式**
- [QQLight](https://www.52chat.cc/download.php) - 拥有免费阉割版
- [Cleverqq(原 IRQQ)](https://www.cleverqq.cn/) - 拥有免费阉割版
- [MyPCQQ](https://mypcqq.cc/) - 免费

> **酷 Q 平台 需要启用*开发者模式*才能看到本插件 (感谢来自一位用户的提醒), 具体启用方法参见 [FAQ](docs/faq.md).**  
> **我们建议你将你选择的机器人放在一台 24 小时运行的电脑/服务器上.**

### 第二步: 安装插件文件

#### 从 GitHub Release / AppVeyor (推荐)

1. 下载 [~
