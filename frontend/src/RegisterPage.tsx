import { ChangeEvent, FC, FormEvent, useState } from 'react';
import {
  RouteComponentProps,
  withRouter,
  Link
  } from 'react-router-dom';
import { Page } from './Page';
import { userRegister } from './User';

export const RegisterPage: FC<RouteComponentProps> = ({
  history,
  location,
}) => {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [passwordVerify, setPasswordVerify] = useState('');

  const handleUsernameInputChange = (e: ChangeEvent<HTMLInputElement>) => {
    setUsername(e.currentTarget.value);
  };
  const handlePasswordInputChange = (e: ChangeEvent<HTMLInputElement>) => {
    setPassword(e.currentTarget.value);
  };
  const handlePasswordVerifyInputChange = (e: ChangeEvent<HTMLInputElement>) => {
    setPasswordVerify(e.currentTarget.value);
  };
  const handleRegisterSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    alert("Hi");
    if (password === passwordVerify) {
      await userRegister(username, password);
    }
  };

  return <Page title="Register">
    <form onSubmit={handleRegisterSubmit}>
      <div>Username: <input type="text" name="username" value={username} onChange={handleUsernameInputChange} /></div>
      <div>Password: <input type="password" name="password" value={password} onChange={handlePasswordInputChange} /></div>
      <div>Password Verify: <input type="password" name="passwordVerify" value={passwordVerify} onChange={handlePasswordVerifyInputChange} /></div>
      <div><button type="submit">Register</button><Link to="/signin" className="Signin">Sign In</Link>
      </div>
    </form>
  </Page>;
}

export const RegisterPageWithRouter = withRouter(RegisterPage);
