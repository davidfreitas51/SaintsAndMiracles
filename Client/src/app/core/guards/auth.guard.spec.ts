import { AuthGuard } from './auth.guard';
import { createRouterSpy } from '../../../testing/router-spy';
import { of } from 'rxjs';
import { UserSessionService } from '../services/user-session.service';
import { createCurrentUser } from '../../../testing/test-fixtures';
import { firstValueFrom } from 'rxjs';

describe('AuthGuard', () => {
  it('allows access when user exists', async () => {
    const session = {
      initUser: jasmine
        .createSpy('initUser')
        .and.returnValue(of(createCurrentUser())),
    } as unknown as UserSessionService;
    const router = createRouterSpy();

    const guard = new AuthGuard(session, router as any);

    const result = await firstValueFrom(guard.canActivate());

    expect(result).toBeTrue();
    expect(router.navigate).not.toHaveBeenCalled();
  });

  it('redirects to login when user is missing', async () => {
    const session = {
      initUser: jasmine.createSpy('initUser').and.returnValue(of(null)),
    } as unknown as UserSessionService;
    const router = createRouterSpy();

    const guard = new AuthGuard(session, router as any);

    const result = await firstValueFrom(guard.canActivate());

    expect(result).toBeFalse();
    expect(router.navigate).toHaveBeenCalledWith(['/account/login']);
  });
});
