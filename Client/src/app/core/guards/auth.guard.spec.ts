import { describe, it, expect, vi } from 'vitest';
import { firstValueFrom, of } from 'rxjs';

import { AuthGuard } from './auth.guard';
import { createRouterSpy } from '../../../testing/router-spy';
import { UserSessionService } from '../services/user-session.service';
import { createCurrentUser } from '../../../testing/test-fixtures';

describe('AuthGuard', () => {
  it('allows access when user exists', async () => {
    const session = {
      initUser: vi.fn().mockReturnValue(of(createCurrentUser())),
    } as unknown as UserSessionService;

    const router = createRouterSpy();

    const guard = new AuthGuard(session, router as any);

    const result = await firstValueFrom(guard.canActivate());

    expect(result).toBe(true);
    expect(router.navigate).not.toHaveBeenCalled();
  });

  it('redirects to login when user is missing', async () => {
    const session = {
      initUser: vi.fn().mockReturnValue(of(null)),
    } as unknown as UserSessionService;

    const router = createRouterSpy();

    const guard = new AuthGuard(session, router as any);

    const result = await firstValueFrom(guard.canActivate());

    expect(result).toBe(false);
    expect(router.navigate).toHaveBeenCalledWith(['/account/login']);
  });
});
