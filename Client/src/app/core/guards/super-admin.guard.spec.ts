import { describe, it, expect, vi } from 'vitest';
import { SuperAdminGuard } from './super-admin.guard';
import { createRouterSpy } from '../../../testing/router-spy';
import { of, firstValueFrom } from 'rxjs';
import { UserSessionService } from '../services/user-session.service';
import { createCurrentUser } from '../../../testing/test-fixtures';

describe('SuperAdminGuard', () => {
  it('allows access for super admins', async () => {
    const session = {
      initUser: vi.fn().mockReturnValue(of(createCurrentUser())),
      getUserRole: vi.fn().mockReturnValue('SuperAdmin'),
    } as unknown as UserSessionService;

    const router = createRouterSpy();

    const guard = new SuperAdminGuard(session, router as any);

    const result = await firstValueFrom(guard.canActivate());

    expect(result).toBe(true);
    expect(router.navigate).not.toHaveBeenCalled();
  });

  it('redirects non-super admins', async () => {
    const session = {
      initUser: vi.fn().mockReturnValue(of(createCurrentUser())),
      getUserRole: vi.fn().mockReturnValue('Admin'),
    } as unknown as UserSessionService;

    const router = createRouterSpy();

    const guard = new SuperAdminGuard(session, router as any);

    const result = await firstValueFrom(guard.canActivate());

    expect(result).toBe(false);
    expect(router.navigate).toHaveBeenCalledWith(['admin/dashboard']);
  });
});
