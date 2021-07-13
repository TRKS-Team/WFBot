# Liunx 安装 WFBot

> 如果你在部署过程中遇到了问题, 请先查看下面的 FAQ. 如果还是无法解决, 可以添加 [QQ 群](http://shang.qq.com/wpa/qunwpa?idkey=1a6da96f714791f3289ee2cafb98847efefd5c5d28e913b6bdf71b8d07e35c53) 或者使用 [GitHub Issues](https://github.com/TRKS-Team/WFBot/issues). 群内问问题请指明你在哪一步遇到了问题.

> 你也可以使用 [**Docker 部署**](docker.md)

[**部署 FAQ (常见问题解答)**](faq.md)

目录:

- [配置 mirai](#第一步-配置-mirai)
- [配置 WFBot](#第二步-配置-wfbot)
  - [如果你想自己编译...](#如果你想自己编译)
- [自定义](#自定义)
  - [启用 WFA 授权 (非必须)](#启用-wfa-授权-非必须)

首先新建两个文件夹, 分别叫 WFBot 和 mirai _并不一定非得是这个名字, 仅以方便演示为主_

## 第一步: 配置 mirai
> mirai 安装教程可能较旧. 总体上来说你需要的只有登录上 mirai 和 mirai-api-http 插件. 

1. 进入 mirai 文件夹.

2. 前往 <https://github.com/iTXTech/mcl-installer/releases> 安装最新版本或自行构建.

```bash
$ cd 你想要安装 iTXTech MCL 的目录
$ curl -LJO https://github.com/iTXTech/mcl-installer/releases/download/72b890d/mcl-installer-72b890d-linux-amd64 # 如果是macOS，就将链接中的 linux 修改为 macos
$ chmod +x mcl-installer-72b890d-linux-amd64
$ ./mcl-installer-72b890d-linux-amd64
```
> - **一路回车就安装成功啦.**
> - **最后一步是下载mcl脚本包，如果没有下载，是网络的问题，请重复执行.**
> - **如果重复执行后还是不下载请自己手动下载[mcl脚本包](https://github.com/iTXTech/mirai-console-loader/releases)解压到目录，更改mcl脚本里的java路径.**

3. 安装完毕后运行一下目录中的mcl, 等待 mirai 输出, 然后关闭 mirai.

4. 打开目录config文件夹里的Console，找到AutoLogin.yml，配置好QQ号及密码保存.![image](https://user-images.githubusercontent.com/52833112/125388125-83923800-e3d1-11eb-9488-5e853ae16472.png)

5. 再运行mcl，无视中间报错，运行完后退出。目录会生成bots文件夹，打开找到你的qq号文件夹，进去之后有个deviceInfo.json文件.
> - **这里的deviceInfo文件需要替换，因为QQ在新设备登录需要验证，Liunx目前不支持滑屏验证**
> - **替换方式建议使用手机下载 [ MiraiAndroid](https://github.com/mzdluo123/MiraiAndroid/releases)**
> - **安装好MiralAndroid后启动，在软件右上角登录QQ（这里需要给软件打开通知栏权限），登录后在左边菜单里下载device.json文件上传替换deviceInfo.json文件即可**
   
5. 下载 mirai-api-http 插件: 从 [mirai-api-http GitHub Release](https://github.com/project-mirai/mirai-api-http/releases/latest) 下载 (中国下载可能较慢), 或 [WFBot 镜像](https://orange-hill-1312.therealkamisama.workers.dev/https://github.com/project-mirai/mirai-api-http/releases/download/v1.9.6/mirai-api-http-v1.9.6.mirai.jar) (版本为 1.9.6, 不一定最新), 下载 mirai-api-http-vx.x.x.mirai.jar, 放入 plugins 文件夹.

6. 再次启动 mirai 并等待输出，这次我们不会报错了，关闭 mirai，这里我们的config文件里会生成net.mamoe.mirai-api-http文件夹.

6. 打开 config\net.mamoe.mirai-api-http\setting.yml 文件. 将 `port` (端口号, 不能大于 65536) 和 `authKey` (连接用密码, 至少 8 位) 修改为一个独特的内容. (后面配置 WFBot 会用到)
 
 ![image](https://user-images.githubusercontent.com/52833112/125391568-4c268a00-e3d7-11eb-84ff-a77a0065e494.png)

7. 执行 `./mcl --update-package net.mamoe:mirai-login-solver-selenium --channel nightly --type plugin`

8. 启动 mirai.
> - **这里我们需要守护进程**

---

## 第二步: 配置 WFBot

1. 安装 .NET Core 3.1 [官方链接](https://docs.microsoft.com/zh-cn/dotnet/core/install/linux) 
   (Linux 用户下载 .NET Core Runtime)

2. 将WFBot文件夹放在安装好的mirai根目录 进入 WFBot 文件夹

3. 下载 WFBot: [链接](https://github.com/TRKS-Team/WFBot/releases/latest). 你需要下载这两个东西:WFBot-Connector-MiraiConnector.7z和WFBot-Linux.7z（

4. 解压: 把 WFBot-Liunx.7z 直接解压, 接着把 WFBot-Connector-MiraiConnector.7z 解压后将文件夹名改为 WFBotConnector 放入WFBot-Liunx根目录内. 
   (确保 MiraiHTTPConnector.dll 直接在 WFBotConnector 内)
   (如没有安装.7z解压命令需自行安装或在自行更改压缩类型）

5. 终端输入dotnet WFBot.dll启动 WFBot.dll

6. 目录会生成 MiraiConfig.json,打开配置好保存.

   ![image](https://user-images.githubusercontent.com/52833112/125391845-b808f280-e3d7-11eb-9881-9769bd2cfd2d.png)

7. 再次打开 WFBot.dll, 就可以运行了.  
> - **注意 你得先打开 mirai, 再打开 WFBot**
> - **建议使用tmux命令守护进程**
> - **词库放在WFOfflineResource文件夹内**

8. 设置界面可以直接修改 WFConfig.json文件.

### 如果你想自己编译...

clone 这个库, 运行 `build-wfbot.bat` 和 `build-connector.bat`, 编译的结果在 out 文件夹内.
如果你是直接下载的这个库, 在 vs 内右键 WFBot, 转到 Build -> Conditional conpliation symbols, 填入 `NoGitVersion`, 编译时使用 `build-wfbot-nogitversion.bat` 来正常编译.

- 针对改代码(如文字提示)又想享受官方编译最新或者自动更新的客户 你可以写一个 WFBot 的 [插件](plugin.md)

> 如果你不需要修改代码, 我们强烈建议你从上面下载.  
> 如果你修改了代码并应用到机器人上, 建议你在 GitHub 上开源其最新版本.  
> **如果你使用非官方版 我们将不保证运行安全与稳定.**

---

## 自定义

> WFBot 控制台内输入 ui 可以打开设置窗口 (仅 Windows) (以后可能会适配全平台)
> ![](images/2021-01-20-23-36-00.png)

可自定义的内容如下:

- 修改群通知功能所用的口令 **(默认为 7 个 \*)**
- 是否需要前导`/`来使用命令 **(默认需要)**
- 包含 哪些奖励的入侵任务 需要通知到群内 **(默认参见设置)**
- 用于管理机器人的 QQ 号 **(填你自己的, 用来修改敏感信息和接收报错)**
- 是否自动同意 别人邀请机器人入群 **(无需群内管理)** 和 自主申请入群 **(需群内管理)**
- WFA 授权的 `ClientId` 和 `ClientSecret` (非必须, 见下)
- WM 商品和紫卡查询单次发送的条数
- 每分钟机器人调用次数限制
- 是否使用中转后的 WarframeMarket 接口 (需 WFA 授权)
- [GithubToken](token.md) **(非必须)**

可以使用的功能如下:

- 对所有 **启用了通知功能** 的群发送一条通知

### 启用 WFA 授权 **(非必须)**

设置内填入从云之幻处授权的 `ClientId` 和 `ClientSecret` 即可启用  
**如果你不知道这俩是干嘛的, 就别瞎填, 因为我的用户创造力都好强啊**

> **不启用授权不影响基本功能**

> **WM 查询** 可使用中转过后的服务器 **速度大概更高**  
> **紫卡市场** 使用 **必须** 启用 WFA 授权

**授权获取** 请查看 **[云之幻的 API 文档](https://www.richasy.cn/wfa-api-apply/)**
