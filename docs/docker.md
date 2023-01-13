# Docker 部署

0. 安装 Docker 可以参阅 <https://yeasy.gitbook.io/docker_practice/install>
1. 找个文件夹, 比如说创一个叫 mirai
2. 新建一个叫 `docker-compose.yml` 的文件, 将 <https://github.com/TRKS-Team/WFBot/blob/universal/docs/docker-compose.yml> 的内容复制到里面;
    > 直接执行 `wget https://ghproxy.com/https://raw.githubusercontent.com/TRKS-Team/WFBot/universal/docs/docker-compose.yml` 也可
3. 首先运行 `sudo docker-compose run --rm mirai` 配置mirai的自动登录和滑动
   > 也可以顺便将原本的Device.json复制到 `mirai/bots/{机器人qq}/` 下, 避免反复出现滑动验证  
   > 你可以先下载[MiraiAndroid](https://github.com/mzdluo123/MiraiAndroid), 在手机上完成登录后, 选择导出Device.json, 然后复制到对应的文件夹下  

   输入 `autoLogin add {机器人QQ} {密码}`  
4. 打开`mirai/config/net.mamoe.mirai-api-http` 修改其中的 `setting.yml` 为以下内容

```yaml
adapters: 
  - http
  - ws
debug: false
enableVerify: true
verifyKey: 请修改我 # 修改为和下文对应的一个Token
singleMode: false
cacheSize: 4096
persistenceFactory: 'built-in'
adapterSettings: {}
```
5. 再次运行 `sudo docker-compose run --rm mirai` 观察到以下绿色输出
![](images/QQ%E6%88%AA%E5%9B%BE20220627214408.png)
> 请顺便确认机器人也已经自动登录  
7. 运行 `sudo docker-compose up -d mirai` 此时 mirai 已经在docker容器内运行
8. 运行 `sudo docker-compose run --rm wfbot` 观察到以下输出  
![](images/QQ%E6%88%AA%E5%9B%BE20220627214621.png)  
打开 `mirai/WFBotConfigs/WFConfig.json` 修改此处内容   
![](images/QQ%E6%88%AA%E5%9B%BE20220627214806.png)  
再次运行 `sudo docker-compose run --rm wfbot` 观察到以下输出  
![](images/QQ%E6%88%AA%E5%9B%BE20220627214916.png)  
打开`mirai/WFBotConfigs/MiraiHTTPv2.json` 修改为以下内容  
![](images/QQ%E6%88%AA%E5%9B%BE20220627215234.png)  
最后再次运行 `sudo docker-compose run --rm wfbot` 可以尝试与机器人进行消息互动确保运行正常  
确保正常后摁下 Ctrl+C 退出 wfbot
9. 运行 `sudo docker-compose up -d wfbot` 此时WFBot也将在docker容器内运行

## 如何启动和更新  
**本教程顺便部署了一个 [Watchtower](https://github.com/containrrr/watchtower/) 实例, 将会每半个小时自动检查 mirai 和 WFBot的更新, 如果有更新将会自动重启容器完成自动更新.**  
想要再次启动, 就直接输入 `sudo docker-compose up -d` , WFBot 将会在 mirai 启动后自动连接.  
想要手动更新 WFBot, 直接输入 `sudo docker-compose pull wfbot && sudo docker-compose up -d`
