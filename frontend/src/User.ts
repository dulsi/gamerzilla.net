import { http } from "./http";

export interface LoginData {
  username: string;
  password: string;
}

export interface UserData {
  userName: string;
  password: string;
  admin: boolean;
  visible: boolean;
  canApprove: boolean;
}

const userDataEmpty: UserData = {
  userName: "",
  password: "",
  admin: false,
  visible: false,
  canApprove: false
};

export const userLogin = async (username: string, password: string):
  Promise<UserData> => {
    try {
      const postBody : LoginData = {username: username, password: password};
      const result = await http<
        LoginData,
        UserData
      >({
        path: '/user/login',
        method: 'post',
        body: postBody
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

export const userRegister = async (username: string, password: string):
  Promise<UserData> => {
    try {
      const postBody : LoginData = {username: username, password: password};
      const result = await http<
        LoginData,
        UserData
      >({
        path: '/user/register',
        method: 'post',
        body: postBody
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

export const getWhoami = async ():
  Promise<UserData> => {
    try {
      const result = await http<
        undefined,
        UserData
      >({
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


export const approve = async (userName: string):
  Promise<boolean> => {
    try {
      await http<
        undefined,
        boolean
      >({
        path: '/user/approve?username=' + userName,
      });
      return true;
    } catch (ex) {
      console.error(ex);
      return false;
    }
  };

export const getUserList = async ():
  Promise<UserData[]> => {
    try {
      const result = await http<
        undefined,
        UserData[]
      >({
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

export const canRegister = async ():
Promise<boolean> => {
  try {
    const result = await http<
      undefined,
      boolean
    >({
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
