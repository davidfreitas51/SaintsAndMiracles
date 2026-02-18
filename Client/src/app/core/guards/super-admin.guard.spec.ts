import { SuperAdminGuard } from './super-admin.guard';
import { createRouterSpy } from '../../../testing/router-spy';
import { of, firstValueFrom } from 'rxjs';
import { UserSessionService } from '../services/user-session.service';
import { createCurrentUser } from '../../../testing/test-fixtures';

describe('SuperAdminGuard', () => {
  it('allows access for super admins', async () => {
    const session = {
      initUser: jasmine
        .createSpy('initUser')
        .and.returnValue(of(createCurrentUser())),
      getUserRole: jasmine
        .createSpy('getUserRole')
        .and.returnValue('SuperAdmin'),
    } as unknown as UserSessionService;
    const router = createRouterSpy();

    const guard = new SuperAdminGuard(session, router as any);

    const result = await firstValueFrom(guard.canActivate());

    expect(result).toBeTrue();
    expect(router.navigate).not.toHaveBeenCalled();
  });

  it('redirects non-super admins', async () => {
    const session = {
      initUser: jasmine
        .createSpy('initUser')
        .and.returnValue(of(createCurrentUser())),
      getUserRole: jasmine.createSpy('getUserRole').and.returnValue('Admin'),
    } as unknown as UserSessionService;
    const router = createRouterSpy();

    const guard = new SuperAdminGuard(session, router as any);

    const result = await firstValueFrom(guard.canActivate());

    expect(result).toBeFalse();
    expect(router.navigate).toHaveBeenCalledWith(['admin/dashboard']);
  });
});
