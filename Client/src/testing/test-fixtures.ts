import { CurrentUser } from '../app/interfaces/current-user';

export const createCurrentUser = (
  overrides: Partial<CurrentUser> = {},
): CurrentUser => ({
  firstName: 'Jane',
  lastName: 'Doe',
  email: 'jane.doe@example.com',
  ...overrides,
});
