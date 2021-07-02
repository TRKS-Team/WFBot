# Docker 部署

0. 安装 Docker 可以参阅 <https://yeasy.gitbook.io/docker_practice/install>
1. 找个文件夹;
2. 新建一个叫 `docker-compose.yml` 的文件;
3. 将 <https://github.com/TRKS-Team/WFBot/blob/universal/docs/docker-compose.yml> 的内容复制到里面;
    > 直接执行 `wget https://ghproxy.com/https://raw.githubusercontent.com/TRKS-Team/WFBot/universal/docs/docker-compose.yml` 也可

4. 要开启 WFBot, 只需要执行 `docker-compose pull && docker-compose run wfbot`
5. 所有配置文件均在 WFBotConfigs 文件夹下, 具体如何配置可以参阅 [**部署指南**](install.md)