import { http } from './http';
import { TokenResult } from './models/TokenResult';

export interface ChangePasswordRequest {
  oldPassword: string;
  newPassword: string;
}

export interface UpdateEmailRequest {
  newEmail: string;
}
export const changePassword = async (oldPass: string, newPass: string): Promise<string> => {
  try {
    const postBody: ChangePasswordRequest = { oldPassword: oldPass, newPassword: newPass };

    const result = await http<ChangePasswordRequest, TokenResult>({
      path: '/account/change-password',
      method: 'post',
      body: postBody,
    });

    return result.parsedBody?.message || 'Password updated successfully.';
  } catch (ex: any) {
    const errorMsg = ex.parsedBody || 'Failed to change password';
    throw new Error(errorMsg);
  }
};

export const updateEmail = async (newEmail: string): Promise<string> => {
  try {
    const postBody: UpdateEmailRequest = { newEmail: newEmail };

    const result = await http<UpdateEmailRequest, TokenResult>({
      path: `/account/update-email`,
      method: 'post',
      body: postBody,
    });

    return result.parsedBody?.message || 'Verification email sent.';
  } catch (ex: any) {
    const errorMsg = ex.parsedBody || 'Failed to send verification email';
    throw new Error(errorMsg);
  }
};
