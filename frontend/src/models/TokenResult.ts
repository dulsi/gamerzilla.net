export interface TokenResult {
  message: string;
  actionType: 'Password' | 'Email' | 'Transfer' | 'Registration';
}
