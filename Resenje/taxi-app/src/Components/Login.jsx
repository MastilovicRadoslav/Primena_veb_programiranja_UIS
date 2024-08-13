import React from 'react';
import '../Styles/LoginPage.css';
import { useState } from 'react';
import { Link } from 'react-router-dom';
import { LoginApiCall } from '../Services/LoginServices';
import { useNavigate } from "react-router-dom";

export default function Login() {
  const [email, setEmail] = useState('');
  const [emailError, setEmailError] = useState('');

  const [password, setPassword] = useState('');
  const [passwordError, setPasswordError] = useState('');
  const loginApiEndpoint = process.env.REACT_APP_LOGIN_API_URL;

  const navigate = useNavigate();

  const handleLoginClick = (e) => {
    e.preventDefault();

    let hasError = false;

    if (!validateEmail(email)) {
      setEmailError("Please enter a valid email address.");
      hasError = true;
    } else {
      setEmailError('');
    }

    if (!validatePassword(password)) {
      setPasswordError("Password needs to have 8 characters, one capital letter, one number, and one special character.");
      hasError = true;
    } else {
      setPasswordError('');
    }

    if (!hasError) {
      LoginApiCall(email, password, loginApiEndpoint)
        .then((responseOfLogin) => {
          if ("Login successful" === responseOfLogin.message) {
            localStorage.setItem('token', responseOfLogin.token);
            navigate("/Dashboard", { state: { user: responseOfLogin.user } });
          } else {
            setEmailError("Invalid email or password, please try again.");
          }
        })
        .catch((error) => {
          setEmailError("Invalid email or password, please try again.");
        });
    }
  };

  const handlePasswordChange = (e) => {
    const value = e.target.value;
    setPassword(value);
    setPasswordError('');
  };

  const handleEmailChange = (e) => {
    const value = e.target.value;
    setEmail(value);
    setEmailError('');
  };

  const validateEmail = (email) => {
    const isValidEmail = /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
    return isValidEmail;
  };

  const validatePassword = (password) => {
    const isValidPassword = /^(?=.*[A-Z])(?=.*[!@#$%^&*])(?=.{8,})/.test(password);
    return isValidPassword;
  };

  return (
    <div className='login-page'>
      <header className="header">
        <h1>BlueTaxi</h1>
      </header>
      <div className="login-container">
        <div className="login-form">
          <h3 className='form-title'>LOGIN</h3>
          <form onSubmit={handleLoginClick} method='post'>
            <input className="input-field" type="text" placeholder="Email" value={email} onChange={handleEmailChange} />
            {emailError && <span className="error-message">{emailError}</span>}
            <input className="input-field" type="password" placeholder="Password" value={password} onChange={handlePasswordChange} />
            {passwordError && <span className="error-message">{passwordError}</span>}
            <button className="login-button" type='submit'>Login</button>
          </form>
          <p className="signup-link">Don't have an account? &nbsp;
            <Link to="/Register" className="register-link">Register</Link>
          </p>
        </div>
      </div>
      <footer className="footer">
        <p>&copy; 2024 BlueTaxi. All rights reserved.</p>
      </footer>
    </div>
  );
}
