import React, { useEffect, useState } from 'react';
import { GoogleLogin } from 'react-google-login';
import '../Styles/RegisterPage.css';
import { gapi } from 'gapi-script';
import { Link } from 'react-router-dom';
import { RegularRegisterApiCall } from '../Services/RegisterServices.js';
import { useNavigate } from 'react-router-dom';

export default function Register() {
    const clientId = process.env.REACT_APP_CLIENT_ID;
    const regularRegisterApiEndpoint = process.env.REACT_APP_REGISTER_API_URL;

    const [firstName, setFirstName] = useState('');
    const [firstNameError, setFirstNameError] = useState(false);

    const [lastName, setLastName] = useState('');
    const [lastNameError, setLastNameError] = useState(false);

    const [birthday, setBirthday] = useState('');
    const [birthdayError, setBirthdayError] = useState(false);

    const [address, setAddress] = useState('');
    const [addressError, setAddressError] = useState(false);

    const [username, setUsername] = useState('');
    const [usernameError, setUsernameError] = useState(false);

    const [email, setEmail] = useState('');
    const [emailError, setEmailError] = useState(false);

    const [password, setPassword] = useState('');
    const [passwordError, setPasswordError] = useState(false);

    const [repeatPassword, setRepeatPassword] = useState('');
    const [repeatPasswordError, setRepeatPasswordError] = useState(false);

    const [typeOfUser, setTypeOfUser] = useState('Driver');
    const [typeOfUserError, setTypeOfUserError] = useState(false);

    const [imageUrl, setImageUrl] = useState(null);
    const [imageUrlError, setImageUrlError] = useState(false);

    const [userGoogleRegister, setUserGoogleRegister] = useState('');
    const navigate = useNavigate();
    const handleRegisterClick = async (e) => {
        e.preventDefault();

        const resultOfRegister = await RegularRegisterApiCall(
            firstNameError,
            lastNameError,
            birthdayError,
            addressError,
            usernameError,
            emailError,
            passwordError,
            repeatPasswordError,
            imageUrlError,
            firstName,
            lastName,
            birthday,
            address,
            email,
            password,
            repeatPassword,
            imageUrl,
            typeOfUser,
            username,
            regularRegisterApiEndpoint
        );
        if (resultOfRegister) {
            alert("Successfully registered!");
            navigate("/");
        }
    };
    
    const handleTypeOfUserChange = (e) => {
        const value = e.target.value;
        setTypeOfUser(value);
        setTypeOfUserError(value.trim() === '');
    };

    const handleInputChange = (setter, errorSetter) => (e) => {
        const value = e.target.value;
        setter(value);
        errorSetter(value.trim() === '');
    };

    const handleEmailChange = (e) => {
        const value = e.target.value;
        setEmail(value);
        const isValidEmail = /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value);
        setEmailError(!isValidEmail);
    };

    const handlePasswordChange = (e) => {
        const value = e.target.value;
        setPassword(value);
        const isValidPassword = /^(?=.*[A-Z])(?=.*[!@#$%^&*])(?=.{8,})/.test(value);
        setPasswordError(!isValidPassword);
    };

    const handleRepeatPasswordChange = (e) => {
        const value = e.target.value;
        setRepeatPassword(value);
        setRepeatPasswordError(value !== password);
    };

    const handleImageUrlChange = (e) => {
        const selectedFile = e.target.files[0];
        setImageUrl(selectedFile || null);
        setImageUrlError(!selectedFile);
    };

    useEffect(() => {
        if (clientId) {
            function start() {
                gapi.client.init({
                    clientId: clientId,
                    scope: ""
                });
            }
            gapi.load('client:auth2', start);
        } else {
            console.error("Client ID is not defined in .env file");
        }
    }, [clientId]);

    const onSuccess = (res) => {
        const profile = res.profileObj;
        setEmail(profile.email);
        setFirstName(profile.givenName);
        setLastName(profile.familyName);
        
        setEmailError(!profile.email);
        setFirstNameError(!profile.givenName);
        setLastNameError(!profile.familyName);

        alert("Please complete other fields!");
    }

    const onFailure = (res) => {
        console.log("Failed to register:", res);
    }

    return (
        <div className='register-page'>
            <header className="header">
                <h1>BlueTaxi</h1>
            </header>
            <div className="register-container">
                <div className="register-form">
                    <h3 className='form-title'>Registration</h3>
                    <hr />
                    <br />
                    <div className="flex flex-col md:flex-row w-max">
                        <form onSubmit={handleRegisterClick} encType="multipart/form-data" method='post'>
                            <div className="grid grid-cols-2 gap-4">
                                <input
                                    className={`input-field ${firstNameError ? 'error' : ''}`}
                                    type="text"
                                    placeholder="First Name"
                                    value={firstName || ''}
                                    onChange={handleInputChange(setFirstName, setFirstNameError)}
                                    required
                                />
                                <input
                                    className={`input-field ${lastNameError ? 'error' : ''}`}
                                    type="text"
                                    placeholder="Last Name"
                                    value={lastName || ''}
                                    onChange={handleInputChange(setLastName, setLastNameError)}
                                    required
                                />
                                <input
                                    className={`input-field ${usernameError ? 'error' : ''}`}
                                    type="text"
                                    placeholder="Username"
                                    value={username || ''}
                                    onChange={handleInputChange(setUsername, setUsernameError)}
                                    required
                                />
                                <input
                                    className={`input-field ${emailError ? 'error' : ''}`}
                                    type="email"
                                    placeholder="Email"
                                    value={email || ''}
                                    onChange={handleEmailChange}
                                    required
                                />
                                <input
                                    className={`input-field ${passwordError ? 'error' : ''}`}
                                    type="password"
                                    title='Password needs 8 characters, one capital letter, number, and special character'
                                    placeholder="Password"
                                    value={password || ''}
                                    onChange={handlePasswordChange}
                                    required
                                />
                                <input
                                    className={`input-field ${repeatPasswordError ? 'error' : ''}`}
                                    type="password"
                                    placeholder="Repeat Password"
                                    value={repeatPassword || ''}
                                    onChange={handleRepeatPasswordChange}
                                    required
                                />
                                <input
                                    className={`input-field ${birthdayError ? 'error' : ''}`}
                                    type="date"
                                    value={birthday || ''}
                                    onChange={handleInputChange(setBirthday, setBirthdayError)}
                                    required
                                />
                                <input
                                    className={`input-field ${addressError ? 'error' : ''}`}
                                    type="text"
                                    placeholder="Address"
                                    value={address || ''}
                                    onChange={handleInputChange(setAddress, setAddressError)}
                                    required
                                />
                                <select
                                    className={`input-field ${typeOfUserError ? 'error' : ''}`}
                                    value={typeOfUser}
                                    onChange={handleTypeOfUserChange}
                                >
                                    <option>Driver</option>
                                    <option>Rider</option>
                                    <option>Admin</option>
                                </select>
                                <input
                                    className={`input-field ${imageUrlError ? 'error' : ''}`}
                                    type="file"
                                    onChange={handleImageUrlChange}
                                    required
                                />
                            </div>
                            <button type="submit" className="btn-primary mt-4">Register</button>
                        </form>
                    </div>
                    <br />
                    <GoogleLogin
                        clientId={clientId}
                        buttonText="Register with Google"
                        onSuccess={onSuccess}
                        onFailure={onFailure}
                        cookiePolicy={'single_host_origin'}
                    />
                    <br />
                    <br />
                    <Link to="/" className='link-underline'>Already registered? <b>Login now!</b></Link>
                </div>
            </div>
            <footer className="footer">
                <p>&copy; 2024 BlueTaxi. All rights reserved.</p>
            </footer>
        </div>
    );
}
