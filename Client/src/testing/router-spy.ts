export const createRouterSpy = (
  url: string = '/',
): { url: string; navigate: jasmine.Spy } => ({
  url,
  navigate: jasmine.createSpy('navigate'),
});
