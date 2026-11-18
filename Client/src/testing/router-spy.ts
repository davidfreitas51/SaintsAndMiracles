import { vi } from 'vitest';

export const createRouterSpy = (
  url = '/',
): { url: string; navigate: ReturnType<typeof vi.fn> } => ({
  url,
  navigate: vi.fn(),
});
