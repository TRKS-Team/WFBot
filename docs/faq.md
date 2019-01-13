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
    尝试不要关闭这个窗口 等待五分钟 如果机器人仍然没有正常启动 就使用Github Issue或者直接加群反馈
