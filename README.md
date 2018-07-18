# ng-alain 与 .net core 搭配

# 本地启动

```bash
git clone --depth 1 https://github.com/cipchk/ng-alain-.netcore
cd be
dotnet run
cd fe
yarn
npm run hmr
```

# 写在前面

鉴于很多使用 ng-alain 都以 .net 为后端，以下我将以一个示例来描述 ng-alain 如何同 .net core 一起开发。示例以单个中后台项目为基准，对于多项目的应用大体相同，但整体目录结构当然不能以单个项目了，更多应该以多人开发为准。

> 所有源码，从 [Github](https://github.com/cipchk/ng-alain-.netcore) 中获取。

# 一、构建项目

## 1、构建

分为 ng-alain 和 .net core 两个部分，当然我推荐二者分开创建在一个根目录下，例如以一个 `asdf` 为项目名，创建一个 `asdf` 目录，然后分别使用以下方式构建一个完整的前后端：

```bash
md asdf
cd asdf
```

**ng-alain**

```bash
ng new asdf --style less
cd asdf
ng add ng-alain
```

此时，我们将 `asdf/asdf` 目录重命名另一个名称以便区分前后端：`fe`。

**.net core**

```bash
dotnet new webapi -o asdf
# 这里采用命令行，依然可以使用 vs 创建项目
```

同样，我们将 `asdf/asdf` 目录重命名另一个名称以便区分前后端：`be`。

> 示例的命名方式你可以使用你喜欢的风格。

最终我们的 `asdf` 项目的目录结构如下：

```
asdf
  - be （后端 .net core）
  - fe （前端 Angular）
```

## 2、运行

前后端分开自然，分别开启两个命令行，而 vscode 着实很方便，分别在 `be` 和 `fe` 目录下运行：

```bash
be: dotnet watch run
fe: npm run hmr
```

默认情况下分开可以通过 https://localhost:5001/api/values 和 http://localhost:4200/ 访问前后端。

**IIS**

其实若是使用 vs 创建项目，可以更友好的将后端绑定至某个域名下，配合修改 host 可以使开发环境更接近生产环境。当然啦，这一部分百度或博客园可以得到信息支持，这里不再赘述。

# 二、编写后端

我在[浅谈Angular网络请求](https://zhuanlan.zhihu.com/p/40014232) 描述过网络请求与用户认证相关的，以此为扩展，我们来尝试怎么实现这些细节。

.net core 也有中间件的概念，如同 Angular 拦截器。依然以一个网络请求流程式来描述这一过程，其大概如下：

- 使用拦截器，从 Header 获取用户 Token，并检查 Token 有效性
- 使用过滤器，检查请求体参数有效性
- 执行方法，并响应结果
- 使用拦截器处理统一异常处理

当然，这只是一个大概性，细节上可能需要处理得更多。

## 1、校验 Token

在 `Startup.cs` 增加一个简单通过 `header` 来获取 `token` 属性值，并进行校验有效性，最后再结果写入至 `context.Items` 里，这样整个请求只需要一次用户信息，后续直接使用 `context.Items` 来获取用户信息。

```cs
app.Use(async (context, next) => {
  if (!context.Request.Path.ToString().StartsWith("/api/passport")) {
    var _token = "";
    if (context.Request.Headers.TryGetValue("token", out var tokens) && tokens.Count > 0) {
      _token = tokens[0];
    }
    if (_token != "asdf") {
      context.Response.StatusCode = 401;
      return ;
    }
    
    var user = new User();
    user.Id = 1;
    user.Name = "cipchk";
    context.Items.Add("token", _token);
    context.Items.Add("user", user);
  }
  await next();
});
```

整段代码实现几个细节：

- 忽略所有 `/api/passport` 开头用户 Token 校验
- 从 headers 获取 `token` 值，并校验值
  - 若错误则直接返回 `401` 不再执行后续动作
- 获取 `User` 信息并写入 `context.Items` 中，以后后续请求直接读取

此时，若再一次访问 https://localhost:5001/api/values 后端接收到的是一个 `401` 状态码。

## 2、统一异常处理

依然可以使用中间件的写法，但为了区分不同，这里采用过滤器的写法。

继承 `ExceptionFilterAttribute` 并重写 `OnException` 就可以简单的完成，`ExceptionFilterAttribute` 异常过滤器会在执行过程中若遇到 `throw` 时会被触发。

```cs
public class ExceptionAttribute : ExceptionFilterAttribute {
  public override void OnException(ExceptionContext context) {
    var res = context.HttpContext.Response;

    res.StatusCode = 200;
    res.ContentType = "application/json; charset=utf-8";
    context.Result = new JsonResult(new {
      msg = context.Exception.Message,
      code = 503
    });
  }
}
```

最后，需要注册到整个应用里。

```cs
services.AddMvc(options => {
  options.Filters.Add(new ExceptionAttribute());
})
```

## 3、PassportController

新建一个 `PassportController.cs` 文件，内容如下：

```cs
[Route("api/[controller]")]
[ApiController]
public class PassportController : ControllerBase
{
  [HttpGet("{id}")]
  public JsonResult Get(int id)
  {
    if (id != 1) throw new Exception("无效用户");
    return new JsonResult(new { msg = "ok", data = "asdf" });
  }
}
```

前面我们已经忽略对所有 `/api/passport` 开头的 URL 的用户 Token 校验，它是一个授权页理当如此。这里体现了两个细节：

- 若无效用户抛出一个错误
  - `ExceptionAttribute` 会捕获到这个错误，并重新指定变更响应体内容
  - 当然你依然可以使用 `return new JsonResult(new { msg = "无效用户" });` 这种方式
- 若有效用户返回用户 Token 值

这里都是手工创建统一响应体，你依然可以利用过滤器或中间件来统一处理响应体为统一风格，而对于方法内永远都只返回一个 `data` 对应值。上述单纯只是一个示例，需要自行更进一步封装。

## 4、跨域问题

本文描述是前后端分开开发，因此开发过程中势必存在跨域请求的问题，一个简单的办法在 `Startup.cs` 里增加跨域代码：

```cs
#if DEBUG
app.UseCors(builder =>
{
  builder.AllowAnyMethod()
          .AllowAnyHeader()
          .AllowAnyOrigin();
});
#endif
```

条件编译可以很好的解决生产和开发环境的不同，因为部署时我们将不存在跨域问题，后续会描述。

## 5、小结

到此，我们已经实现大部分 [浅谈Angular网络请求](https://zhuanlan.zhihu.com/p/40014232) 描述的功能，哪怕都是一个简化，但我们可以访问：

- https://localhost:5001/api/values 返回 401
- https://localhost:5001/api/passport/1 返回用户 Token
- https://localhost:5001/api/passport/2 返回无效用户异常

# 三、编写前端

ng-alain 默认是以尽可能接收生产环境项目的脚手架，诚如我在 [浅谈Angular网络请求](https://zhuanlan.zhihu.com/p/40014232) 描述的那样，不应该编写一个简单的 Hello World 请求来校验是否可用。

在开始之前，需要先了解 Angular 环境变更配置，它位于 `fe/src/environments` 目录中，每一个文件表示一种环境，他们有者相同参数，但其值可能各不同。

若是你跟着本文来做的话，那么相对应的是 `environment.hmr.ts`，我们修改这里的 `SERVER_URL` 值为 `https://localhost:5001/api/`。这是表示所有请求都会在请求URL前自动加上该地址。

## 1、APP_INITIALIZER

Angular 启用前我们能做的事只有这里，脚手架默认实现了 `StartupService`（位于：`fe/src/app/core/startup/` 下），当然默认代码并不可用，我们将其修改为：

```ts
load(): Promise<any> {
  // only works with promises
  // https://github.com/angular/angular/issues/15088
  return new Promise((resolve, reject) => {
    this.httpClient.get('values').subscribe(
      (res: any) => {
        this.injector.get(NzMessageService).success(JSON.stringify(res));
      },
      () => {},
      () => {
        resolve(null);
      },
    );
  });
}
```

这里请求是 `api/values`，但由于我们给 Angular 环境变量统一配置 URL 前缀，因此只需要一个简单的 `values` 为请求 URL。（注：若请求URL地址不是期望结果，需要重新运行 `npm run hmr`）

此时，你访问前端时会自动跳转至 `/passport/login` 登录页，这一切都是由于 `@delon/auth` 用户认证模块在管理的，我们没有写任何一行关于前端校验的代码。

## 2、登录页

登录示例页的大部分代码是可用的，但本文并不关心这一些，我们修改其中发送请求部分如下：

```ts
// mock http
this.loading = true;
this.http.get('passport/1').subscribe((res: any) => {
  this.loading = false;
  // 清空路由复用信息
  this.reuseTabService.clear();
  // 设置Token信息
  this.tokenService.set({
    token: res.data,
  });
  // 重新获取 StartupService 内容，若其包括 User 有关的信息的话
  this.startupSrv.load().then(() => this.router.navigate(['/']));
  // 否则直接跳转
  // this.router.navigate(['/']);
});
```

请求 `passport/1` 返回用户 Token，并把 Token 值写入 `TokenService` 中，最后跳转至仪表盘页。

登录成功后，你还会接收到一个条 `[ "value1", "value2" ]` 的消息，这是来自 `APP_INITIALIZER` 产生的，至少表明我们能正常访问到 `values`，因为此时你会发现该主体的 `Header` 已经包含了 `token: asdf` 字样。

## 3、小结

关于前端部分我简略的描述，这里还有更多细节，例如：响应体直接返回 `data` 的内容，更多细节自行挖掘。

# 四、部署

示例中的 .net core 部分是单纯的 Web API 项目，由此不存在任何页面之说。而如何让 .net core 项目直接以 Angular 项目来访问呢？这需要解决两个问题。

- 前端打包至 `wwwroot` 目录里
- .net core 默认以 `index.html` 默认访问页

## 1、打包前端

这里一个简单的办法是修改 `angular.json` 的输出路径（当然也可以直接命令行里直接指定）：

```json
"outputPath": "../be/wwwroot",
```

最后，执行：

```bash
npm run bash
```

打包后的文件会直接存放至 `be/wwwroot` 目录下。

## 2、打包后端

默认情况下需要开启静态资源访问能力并且指定默认页为 `index.html`，在 `Startup.cs` 增加：

```cs
var options = new DefaultFilesOptions();
options.DefaultFileNames.Clear();
options.DefaultFileNames.Add("index.html");
app.UseDefaultFiles(options);

app.UseStaticFiles();
```

此时，若你再访问 https://localhost:5001/ 会发现我们启动的是一个 ng-alain 项目。

而对于后端的打包，也非常简单：

```bash
dotnet publish -o ../dist
```

最终，直接将根目录下的 `dist` 部署至 Web 服务器上。

# 五、总结

本文算是对 [浅谈Angular网络请求](https://zhuanlan.zhihu.com/p/40014232) 进一步实践，虽然一切都采用简化的代码来解释，但总体的流程是等同的。

基于 .net core 为后端是出于群里反应很多项目都是使用 .net，可能是 .net framework 版本，但本质上是相同。示例我是以 vscode 编写，可能后端的格式会有些同 vs 不同。

（完）