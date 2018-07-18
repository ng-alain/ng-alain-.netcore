import { Injectable, Injector, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { MenuService, SettingsService, TitleService } from '@delon/theme';
import { DA_SERVICE_TOKEN, ITokenService } from '@delon/auth';
import { ACLService } from '@delon/acl';
import { NzMessageService } from 'ng-zorro-antd';
import { catchError } from 'rxjs/operators';

/**
 * 用于应用启动时
 * 一般用来获取应用所需要的基础数据等
 */
@Injectable()
export class StartupService {
  constructor(
    private menuService: MenuService,
    private settingService: SettingsService,
    private aclService: ACLService,
    private titleService: TitleService,
    @Inject(DA_SERVICE_TOKEN) private tokenService: ITokenService,
    private httpClient: HttpClient,
    private injector: Injector,
  ) {}

  load(): Promise<any> {
    // only works with promises
    // https://github.com/angular/angular/issues/15088
    return new Promise((resolve, reject) => {
      this.httpClient
        .get('values')
        .pipe(
          catchError((res: any) => {
            resolve(null);
            return null;
          }),
        )
        .subscribe(
          (res: any) => {
            if (res != null) this.injector.get(NzMessageService).success(JSON.stringify(res));
            // // application data
            // const res: any = appData;
            // // 应用信息：包括站点名、描述、年份
            // this.settingService.setApp(res.app);
            // // 用户信息：包括姓名、头像、邮箱地址
            // this.settingService.setUser(res.user);
            // // ACL：设置权限为全量
            // this.aclService.setFull(true);
            // // 初始化菜单
            // this.menuService.add(res.menu);
            // // 设置页面标题的后缀
            // this.titleService.suffix = res.app.name;
          },
          () => {},
          () => {
            resolve(null);
          },
        );
    });
  }
}
