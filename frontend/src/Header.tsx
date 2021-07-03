import React from 'react';
import { Link } from 'react-router-dom';
import { UserIcon } from './Icons';
import './Header.css';

export const Header = () => (
  <div className="Header">
    <Link to="/" className="Headername">Gamerzilla</Link>
    <Link to="/signin" className="Signin">
      <UserIcon />
      <span>Sign In</span>
    </Link>
  </div>
);
