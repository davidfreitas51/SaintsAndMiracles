import { bootstrapApplication } from '@angular/platform-browser';
import { AppComponent } from './app/app.component';
import { appConfig } from './app/app.config';
import { provideEnvironmentNgxMask } from 'ngx-mask';
import { provideCharts, withDefaultRegisterables } from 'ng2-charts';

bootstrapApplication(AppComponent, {
  providers: [
    ...appConfig.providers,
    provideEnvironmentNgxMask({ validation: true }),
    provideCharts(withDefaultRegisterables())
  ],
}).catch((err) => console.error(err));
