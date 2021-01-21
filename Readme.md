> 这是一个开发计划
- `~` 代表 winds 根目录
- `@` 代表所有用户根目录 (`~\data\user`)

#### 服务主机
- 涉及路径的字符串一律使用小写
- 数据文件根目录是 `~\data`，如果指定的目录无读写权限则直接异常，建议不要指定其他具体路径，使用链接 `mklink.exe /?` 替代
- 用户文件根目录是 `~\users`，如果指定的目录无读写权限则直接异常
- 日志文件根目录是 `~\logs`，如果指定的目录无读写权限则直接异常
- 单元文件根目录是 `~\units`，如果指定的目录无读写权限则直接异常
- 必须在 LocalSystem 权限下运行，否则退出（调试模式除外）
- 操作使用 HTTP 请求
- 在线终端使用 WebSocket
- 在线修改单元配置，使用 HTTP 请求
- 在线传输文件，使用 HTTP 请求
- 在线编辑部分文件（指定后缀名，不超过 128KiB），使用 HTTP 请求

#### 多用户
- 用户配置文件是 `~\users\$username.json`，密码以明文存在，这个文件如果能被读取，那么任何加密都已经失去意义
- 只有管理员可以添加普通用户，不存在普通用户自主录入方式
- 管理员可以修改用户密码、描述、备注等，普通用户只能修改密码和描述
- 用户数据根目录是如果未做指定则是 `~\data\user`，如果指定的目录无读写权限则直接异常，不可回退缺省配置
- 管理员用户数据根目录固定使用 `~\data\system`，如果指定的目录无读写权限则直接异常
- 某个用户的数据根目录是 `@\$username`，比如 `C:\winds\data\user\user1` 或 `V:\user9`

#### 托管单元
- 单元的数据根目录是 `~\data\$unituuid`，比如 `C:\winds\data\000000000000` 或 `O:\111111111111`
- 单元的数据根目录也是在线文件管理的完全限定目录，不可突破此目录向上访问
- 单元的配置位于 `~\units\$unituuid.json` => `Name|Description|Username|Password|WorkDirectory|StartCommand|StartArguments|StopCommand|StopArguments|RestartCommand|RestartArguments|AutoStart|AutoStartDelay|RestartWhenException|Priority|Affinity|Environments|MonitorProcessorUsage|MonitorMemoryUsage|MonitorNetworkUsage`，只能通过表单编辑
- 单元支持不同的启动/停止/重启指令和参数，比如 nginx -s quit
- 管理员的单元可选 LocalSystem/LocalService/NetworkService 或手动输入操作系统的用户名密码，比如 Administrator
- 普通用户的单元可选 LocalService/NetworkService 或手动输入操作系统的用户名密码，比如 User3
- 单元的 stdout/stderr 作为日志存放在 `~\logs\$unituuid\$date[_error].log`，使用单独的页面管理，可查看最后 N 行或直接下载
- 一个单元可以同时分配给多个用户，分配时应该检查是否已属于某个用户并提醒管理员
- 管理员无需分配，可直接管理所有单元
