import React, { useState, useEffect } from 'react';
import { useNavigate } from "react-router-dom";
import '../Styles/DashboardPage.css';
import { FaSignOutAlt } from 'react-icons/fa';
import { GetAllDrivers } from '../Services/AdminServices.js';
import DriversView from './Drivers.jsx';
import { makeImage, convertDateTimeToDateOnly, changeUserFields } from '../Services/ProfileServices.js';
import { getUserInfo } from '../Services/ProfileServices.js';
import VerifyDrivers from './VerifyDrivers.jsx';
import RidesAdmin from './RidesAdmin.jsx';

export default function DashboardAdmin(props) {//user iz dashboard
    const user = props.user; //preko Login pa Dashboard
    const apiEndpoint = process.env.REACT_APP_CHANGE_USER_FIELDS;
    const userId = user.id; //preko Login pa Dashboard
    const jwt = localStorage.getItem('token');
    const navigate = useNavigate();

    const apiForCurrentUserInfo = process.env.REACT_APP_GET_USER_INFO;
    const [currentUser, setUserInfo] = useState('');

    const [address, setAddress] = useState('');
    const [averageRating, setAverageRating] = useState('');
    const [birthday, setBirthday] = useState('');
    const [email, setEmail] = useState('');
    const [firstName, setFirstName] = useState('');
    const [imageFile, setImageFile] = useState('');
    const [isBlocked, setIsBlocked] = useState('');
    const [isVerified, setIsVerified] = useState('');
    const [lastName, setLastName] = useState('');
    const [numOfRatings, setNumOfRatings] = useState('');
    const [password, setPassword] = useState('');
    const [roles, setRoles] = useState('');
    const [status, setStatus] = useState('');
    const [sumOfRatings, setSumOfRatings] = useState('');
    const [username, setUsername] = useState('');

    const [view, setView] = useState('editProfile'); //koristi se za upravljanje koji deo dashboard-a je trenutno prikazan, inicijalno je editProfile, znaci renderuje se na odredjene strancie na osnovu view stanja
    const [isEditing, setIsEditing] = useState(false); //da li se edituje profil?

    const [oldPassword, setOldPassword] = useState('');
    const [newPassword, setNewPassword] = useState('');
    const [repeatNewPassword, setRepeatNewPassword] = useState('');
    const [selectedFile, setSelectedFile] = useState(null);

    const [initialUser, setInitialUser] = useState({});

    useEffect(() => { //prvo sto se poziva na stranici jeste da se ucita korisnikove informacije i prikazu u sekciji prikaz profila
        const fetchUserInfo = async () => {
            try {
                const userInfo = await getUserInfo(jwt, apiForCurrentUserInfo, userId); //preuzimam korisnicke podatke na osnovu JWT tokena i userId
                const user = userInfo.user; //admin
                setUserInfo(user); //ovde smestam
                setInitialUser(user); //inicijalni
                //prikaz informacija u formi za uredjivanje profila
                setAddress(user.address);
                setAverageRating(user.averageRating);
                setBirthday(convertDateTimeToDateOnly(user.birthday));
                setEmail(user.email);
                setFirstName(user.firstName);
                setImageFile(makeImage(user.imageFile));
                setIsBlocked(user.isBlocked);
                setIsVerified(user.isVerified);
                setLastName(user.lastName);
                setNumOfRatings(user.numOfRatings);
                setPassword(user.password);
                setRoles(user.roles);
                setStatus(user.status);
                setSumOfRatings(user.sumOfRatings);
                setUsername(user.username);
            } catch (error) {
                console.error('Error fetching user info:', error.message);
            }
        };

        fetchUserInfo();
    }, [jwt, apiForCurrentUserInfo, userId]); //kad se nesto promijeni automatski se opet ucita

    const handleSaveClick = async () => { //poziv API servisa za azuriranje korisnickih podataka
        const ChangedUser = await changeUserFields(apiEndpoint, firstName, lastName, birthday, address, email, password, selectedFile, username, jwt, newPassword, repeatNewPassword, oldPassword, userId); //azuriranje korisnickih podataka na serveru
        setInitialUser(ChangedUser); //novi podaci se vracaju u kompoennetu i koriste za azuriranje prikaza
        setUserInfo(ChangedUser);
        setAddress(ChangedUser.address);
        setAverageRating(ChangedUser.averageRating);
        setBirthday(convertDateTimeToDateOnly(ChangedUser.birthday));
        setEmail(ChangedUser.email);
        setFirstName(ChangedUser.firstName);
        setImageFile(makeImage(ChangedUser.imageFile));
        setIsBlocked(ChangedUser.isBlocked);
        setIsVerified(ChangedUser.isVerified);
        setLastName(ChangedUser.lastName);
        setNumOfRatings(ChangedUser.numOfRatings);
        setPassword(ChangedUser.password);
        setRoles(ChangedUser.roles);
        setStatus(ChangedUser.status);
        setSumOfRatings(ChangedUser.sumOfRatings);
        setUsername(ChangedUser.username);
        setOldPassword('');
        setNewPassword('');
        setRepeatNewPassword('');
        setIsEditing(false); //rezim uredjivanja se iskljucuje
    }

    const handleSignOut = () => {
        localStorage.removeItem('token');
        navigate('/');
    };

    const handleShowDrivers = () => setView('drivers'); //prikazuje se lista vozaca

    const handleShowDriversForVerification = () => setView('verify'); //prikazuje se verifikacija vozaca

    const handleShowAllRides = () => setView('rides'); //prikazuje se pregled voznji

    const handleEditProfile = () => setView('editProfile'); //prikazuje se prikaz profila

    const handleEditClick = () => setIsEditing(true); //aktivira rezim uredjivanja profila

    const handleCancelClick = () => { //ponistava promene i vraca prehodne inicjalne vrednosti
        setIsEditing(false); //rezim uredjivanja nije ukljucen
        setAddress(initialUser.address);
        setAverageRating(initialUser.averageRating);
        setBirthday(convertDateTimeToDateOnly(initialUser.birthday));
        setEmail(initialUser.email);
        setFirstName(initialUser.firstName);
        setImageFile(makeImage(initialUser.imageFile));
        setIsBlocked(initialUser.isBlocked);
        setIsVerified(initialUser.isVerified);
        setLastName(initialUser.lastName);
        setNumOfRatings(initialUser.numOfRatings);
        setPassword(initialUser.password);
        setRoles(initialUser.roles);
        setStatus(initialUser.status);
        setSumOfRatings(initialUser.sumOfRatings);
        setUsername(initialUser.username);
        setOldPassword('');
        setNewPassword('');
        setRepeatNewPassword('');
        setSelectedFile(null);
    };

    const handleImageChange = (e) => {
        const file = e.target.files[0];
        setSelectedFile(file);
        const reader = new FileReader();
        reader.onloadend = () => setImageFile(reader.result);
        if (file) reader.readAsDataURL(file);
    };

    return (
        <div style={{ display: 'flex', flexDirection: 'column', height: '100vh'}}>
            <div className="navbar">
                <div className="nav-left">
                    <span className="username-text">{username}:</span>
                </div>
                <div className="nav-center">
                    <button className="button" onClick={handleEditProfile}>Profile</button>
                    <button className="button" onClick={handleShowDriversForVerification}>Verify drivers</button>
                    <button className="button" onClick={handleShowDrivers}>Drivers</button>
                    <button className="button" onClick={handleShowAllRides}>Rides</button>
                </div>
                <div className="nav-right">
                    <button className="button-logout" onClick={handleSignOut}>Sign out</button>
                </div>
            </div>
            <div style={{ display: 'flex', flexDirection: 'row', flex: 1, justifyContent: 'center', backgroundImage: 'url("/public/Images/Pozadina.jpg")' }}>
                <div className="edit-profile-container">
                    {view === 'editProfile' && (
                        <div>
                            <div className="edit-profile-header">Edit profile</div>
                            <img src={imageFile} alt="User" className="profile-picture" />
                            {isEditing && <input type="file" onChange={handleImageChange} />}
                            <div className="profile-info">
                                <div>
                                    <label>Username</label>
                                    {isEditing ? (
                                        <input type="text" value={username} onChange={(e) => setUsername(e.target.value)} />
                                    ) : (
                                        <div className="info-text">{username}</div>
                                    )}
                                </div>
                                <div>
                                    <label>First name</label>
                                    {isEditing ? (
                                        <input type="text" value={firstName} onChange={(e) => setFirstName(e.target.value)} />
                                    ) : (
                                        <div className="info-text">{firstName}</div>
                                    )}
                                </div>
                                <div>
                                    <label>Last name</label>
                                    {isEditing ? (
                                        <input type="text" value={lastName} onChange={(e) => setLastName(e.target.value)} />
                                    ) : (
                                        <div className="info-text">{lastName}</div>
                                    )}
                                </div>
                                <div>
                                    <label>Address</label>
                                    {isEditing ? (
                                        <input type="text" value={address} onChange={(e) => setAddress(e.target.value)} />
                                    ) : (
                                        <div className="info-text">{address}</div>
                                    )}
                                </div>
                                <div>
                                    <label>Birthday</label>
                                    {isEditing ? (
                                        <input type="text" value={birthday} onChange={(e) => setBirthday(e.target.value)} />
                                    ) : (
                                        <div className="info-text">{birthday}</div>
                                    )}
                                </div>
                                <div>
                                    <label>Email</label>
                                    {isEditing ? (
                                        <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} />
                                    ) : (
                                        <div className="info-text">{email}</div>
                                    )}
                                </div>
                                <div>
                                    <label>Old password</label>
                                    {isEditing ? (
                                        <input type="password" value={oldPassword} onChange={(e) => setOldPassword(e.target.value)} />
                                    ) : (
                                        <div className="info-text"><input type="password" placeholder="********" disabled /></div>
                                    )}
                                </div>
                                <div>
                                    <label>New password</label>
                                    {isEditing ? (
                                        <input type="password" value={newPassword} onChange={(e) => setNewPassword(e.target.value)} />
                                    ) : (
                                        <div className="info-text"><input type="password" placeholder="********" disabled /></div>
                                    )}
                                </div>
                                <div>
                                    <label>Repeat new password</label>
                                    {isEditing ? (
                                        <input type="password" value={repeatNewPassword} onChange={(e) => setRepeatNewPassword(e.target.value)} />
                                    ) : (
                                        <div className="info-text"><input type="password" placeholder="********" disabled /></div>
                                    )}
                                </div>
                            </div>
                            {isEditing ? (
                                <div className="edit-profile-buttons">
                                    <button className="edit-button" onClick={handleSaveClick}>Save</button>
                                    <button className="cancel-button" onClick={handleCancelClick}>Cancel</button>
                                </div>
                            ) : (
                                <button className="edit-button" onClick={handleEditClick}>Edit</button>
                            )}
                        </div>
                    )}
                    {view === 'drivers' && <DriversView />}
                    {view === 'verify' && <VerifyDrivers />}
                    {view === 'rides' && <RidesAdmin />}
                </div>
            </div>
        </div>
    );
}
