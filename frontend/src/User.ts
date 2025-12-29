import { http } from './http';

export interface LoginData {
  username: string;
  password: string;
  email?: string;
}

export interface ServerOptions {
  canRegister: boolean;
  allowPasswordChange: boolean;
  allowEmailChange: boolean;
  emailEnabled: boolean;
}

export const defaultServerOptions: ServerOptions = {
  canRegister: false,
  allowPasswordChange: false,
  allowEmailChange: false,
  emailEnabled: false,
};

export interface UserData {
  id: number;
  userName: string;
  password: string;
  admin: boolean;
  visible: boolean;
  canApprove: boolean;
  approved: boolean;
  email: string;
}

const userDataEmpty: UserData = {
  id: 0,
  userName: '',
  password: '',
  admin: false,
  visible: false,
  canApprove: false,
  approved: false,
  email: '',
};

export const userLogin = async (username: string, password: string): Promise<UserData> => {
  try {
    const postBody: LoginData = { username: username, password: password };
    const result = await http<LoginData, UserData>({
      path: '/user/login',
      method: 'post',
      body: postBody,
    });
    if (result.parsedBody) {
      return result.parsedBody;
    } else {
      return userDataEmpty;
    }
  } catch (ex) {
    console.error(ex);
    return userDataEmpty;
  }
};

export const userLogout = async (): Promise<boolean> => {
  try {
    await http<undefined, boolean>({
      path: '/user/logout',
      method: 'post',
    });
    return true;
  } catch (ex) {
    console.error(ex);
    return false;
  }
};

export const requestAuthToken = async (identifier: string): Promise<string> => {
  const result = await http<{ identifier: string }, string>({
    path: '/account/request-token',
    method: 'post',
    body: { identifier: identifier },
  });

  return result.parsedBody || 'Success';
};

export const userRegister = async (
  username: string,
  password: string,
  email: string,
): Promise<UserData> => {
  const postBody: LoginData = { username: username, password: password, email: email };

  const result = await http<LoginData, UserData>({
    path: '/user/register',
    method: 'post',
    body: postBody,
  });

  if (result.parsedBody) {
    return result.parsedBody;
  } else {
    return userDataEmpty;
  }
};

export const getWhoami = async (): Promise<UserData> => {
  try {
    const result = await http<undefined, UserData>({
      path: '/user/whoami',
    });
    if (result.parsedBody) {
      return result.parsedBody;
    } else {
      return userDataEmpty;
    }
  } catch (ex) {
    console.error(ex);
    return userDataEmpty;
  }
};

export const approve = async (userName: string): Promise<void> => {
  await http<undefined, boolean>({
    path: '/user/approve?username=' + userName,
    method: 'post',
  });
};

export const demoteAdmin = async (userName: string): Promise<void> => {
  await http({
    path: '/user/demote?username=' + userName,
    method: 'post',
  });
};

export const promoteToAdmin = async (userName: string): Promise<void> => {
  await http({
    path: '/user/promote?username=' + userName,
    method: 'post',
  });
};

export const deleteUser = async (userName: string): Promise<void> => {
  await http({
    path: '/user/delete?username=' + userName,
    method: 'post',
  });
};

export const getServerOptions = async (): Promise<ServerOptions> => {
  try {
    const result = await http<undefined, ServerOptions>({
      path: '/config/options',
      method: 'GET',
    });

    if (result.parsedBody) {
      return result.parsedBody;
    }
    return defaultServerOptions;
  } catch (ex) {
    console.error(ex);
    return defaultServerOptions;
  }
};

export const getUserList = async (): Promise<UserData[]> => {
  try {
    const result = await http<undefined, UserData[]>({
      path: '/user',
    });
    if (result.parsedBody) {
      return result.parsedBody;
    } else {
      return [];
    }
  } catch (ex) {
    console.error(ex);
    return [];
  }
};

export const canRegister = async (): Promise<boolean> => {
  try {
    const result = await http<undefined, boolean>({
      path: '/user/canregister',
    });
    if (result.parsedBody) {
      return result.parsedBody;
    } else {
      return false;
    }
  } catch (ex) {
    console.error(ex);
    return false;
  }
};

export const visible = async (userName: string, val: number): Promise<boolean> => {
  try {
    await http<undefined, boolean>({
      path: '/user/visible?username=' + userName + '&val=' + val,
      method: 'post',
    });
    return true;
  } catch (ex) {
    console.error(ex);
    return false;
  }
};
