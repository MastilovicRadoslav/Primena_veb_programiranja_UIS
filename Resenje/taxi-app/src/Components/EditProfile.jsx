import React, { useState, useEffect } from 'react';
import '../Styles/ProfilePage.css';
import { makeImage, convertDateTimeToDateOnly, changeUserFields, getUserInfo } from '../Services/ProfileServices.js';

const EditProfile = ({ user }) => {
    const userId = user.id;
    const jwt = localStorage.getItem('token');
    const apiForCurrentUserInfo = process.env.REACT_APP_GET_USER_INFO;
    const apiEndpoint = process.env.REACT_APP_CHANGE_USER_FIELDS;

    const [currentUser, setUserInfo] = useState({});
    const [initialUser, setInitialUser] = useState({});
    const [isEditing, setIsEditing] = useState(false);

    // State variables for form fields
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
    const [oldPassword, setOldPassword] = useState('');
    const [newPassword, setNewPassword] = useState('');
    const [repeatNewPassword, setRepeatNewPassword] = useState('');
    const [selectedFile, setSelectedFile] = useState(null);

    useEffect(() => {
        const fetchUserInfo = async () => {
            try {
                const userInfo = await getUserInfo(jwt, apiForCurrentUserInfo, userId);
                const user = userInfo.user;
                setUserInfo(user);
                setInitialUser(user);

                // Set state variables with fetched user data
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
    }, [jwt, apiForCurrentUserInfo, userId]);

    const handleSaveClick = async () => {
        const ChangedUser = await changeUserFields(apiEndpoint, firstName, lastName, birthday, address, email, password, selectedFile, username, jwt, newPassword, repeatNewPassword, oldPassword, userId);
        setInitialUser(ChangedUser);
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
        setIsEditing(false);
    }

    const handleEditClick = () => setIsEditing(true);
    const handleCancelClick = () => {
        setIsEditing(false);
        // Reset state to initial values
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
        <div className="edit-profile-container">
            <div className="edit-profile-header">Edit Profile</div>
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
    );
};

export default EditProfile;
