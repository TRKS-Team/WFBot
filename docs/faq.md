# 常见问题

## Q: 我在酷 Q 中根本看不到这个插件

- A: 启用开发者模式. (下面的图片来自 Mahua.)
  ![CQ](images/CQ.png)

---

## Q: LoadLibrary 失败

![LoadLibraryFailed](images/LoadLibraryFailed.png)

- A: 下载 [.NET Framework 4.6.2](https://dotnet.microsoft.com/download/thank-you/net462) 并安装.

---

## Q: 我是自己编译的机器人, 部署后直接爆炸

- A: 删除 `YUELUO\TRKS.WF.QQBot\TRKS.WF.QQBot.dll.config`.

---

## Q: 应用加载错误 - TRKS.WF.QQBot.dll 读取错误或超时

![Timeout](images/Timeout.png)

- A: 本弹窗的原因很复杂 最有可能是的是因为服务器配置太低导致读取时间过长  
   尝试不要关闭这个窗口 等待五分钟 如果机器人仍然没有正常启动 就使用 Github Issue 或者直接加群反馈

---

## Q: 无法启动机器人, log 下报错：尝试从一个网络位置加载程序集

- A: 在解压酷 Q 及插件文件之前, 先选择解除锁定，并点击确定，再解压压缩文件。

![Unlock](images/Unlock.png)

---

## Q: 我的机器人发出来的时间怎么是英文的

![Languagebug](images/Languagebug.png)

- A: 请将 `YUELUO\TRKS.WF.QQBot\zh-CN` 这个文件夹扔到机器人的根目录.  
    TODO:永久修复这个问题  