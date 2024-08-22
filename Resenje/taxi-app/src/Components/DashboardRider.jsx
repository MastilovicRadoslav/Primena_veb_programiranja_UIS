import React, { useState, useEffect } from 'react';
import { MdPerson } from 'react-icons/md';
import { FaCar, FaRoad, FaSignOutAlt, FaStar } from 'react-icons/fa';
import { useNavigate } from "react-router-dom";
import { makeImage, convertDateTimeToDateOnly, changeUserFields } from '../Services/ProfileServices.js';
import { getUserInfo } from '../Services/ProfileServices.js';
import '../Styles/DashboardPage.css';
import '../Styles/ProfilePage.css';
import '../Styles/NewDrivePage.css';
import { getEstimation, AcceptDrive, convertTimeStringToMinutes } from '../Services/PredictionServices.js';
import { getCurrentRide } from '../Services/RiderServices.js';
import RidesRider from './RidesRider.jsx';
import Rate from './Rate.jsx';

export default function RiderDashboard(props) {
    const user = props.user;
    const jwt = localStorage.getItem('token');
    const navigate = useNavigate();

    const apiEndpoint = process.env.REACT_APP_CHANGE_USER_FIELDS;
    const apiForCurrentUserInfo = process.env.REACT_APP_GET_USER_INFO;
    const apiEndpointEstimation = process.env.REACT_APP_GET_ESTIMATION_PRICE;
    const apiEndpointAcceptDrive = process.env.REACT_APP_ACCEPT_SUGGESTED_DRIVE;
    const apiEndpointForCurrentDrive = process.env.REACT_APP_CURRENT_TRIP;

    const userId = user.id;
    localStorage.setItem("userId", userId);

    const [destination, setDestination] = useState('');
    const [currentLocation, setCurrentLocation] = useState('');
    const [estimation, setEstimation] = useState('');
    const [driversArivalSeconds, setDriversArivalSeconds] = useState('');

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

    const [view, setView] = useState('editProfile');
    const [isEditing, setIsEditing] = useState(false);

    const [oldPassword, setOldPassword] = useState('');
    const [newPassword, setNewPassword] = useState('');
    const [repeatNewPassword, setRepeatNewPassword] = useState('');
    const [selectedFile, setSelectedFile] = useState(null);
    const [initialUser, setInitialUser] = useState({});
    const [clockSimulation, setClockSimulation] = useState('');

    const handleSignOut = () => {
        localStorage.removeItem('token');
        navigate('/');
    };

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
    };

    const handleEditClick = () => setIsEditing(true);
    const handleNewDriveClick = () => setView('newDrive');
    const handleEditProfile = () => setView('editProfile');
    const handleDriveHistory = () => setView('driveHistory');
    const handleRateTrips = () => setView('rateTrips');

    const handleImageChange = (e) => {
        const file = e.target.files[0];
        setSelectedFile(file);
        const reader = new FileReader();
        reader.onloadend = () => setImageFile(reader.result);
        if (file) reader.readAsDataURL(file);
    };

    const handleCancelClick = () => {
        setIsEditing(false);
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

    const handleEstimationSubmit = async () => {
        try {
            if (destination === '' || currentLocation === '') alert("Please complete form!");
            else {
                const data = await getEstimation(jwt, apiEndpointEstimation, currentLocation, destination);
                const roundedPrice = parseFloat(data.price.estimatedPrice).toFixed(2);
                setDriversArivalSeconds(convertTimeStringToMinutes(data.price.driversArivalSeconds));
                setEstimation(roundedPrice);
            }
        } catch (error) {
            console.error("Error when I try to show profile", error);
        }
    };

    const handleAcceptDriveSubmit = async () => {
        try {
            const data = await AcceptDrive(apiEndpointAcceptDrive, userId, jwt, currentLocation, destination, estimation, true, driversArivalSeconds);
            if (data.message === "Request failed with status code 400") {
                alert("You have already submitted ticket!");
                setDriversArivalSeconds('');
                setEstimation('');
                setCurrentLocation('');
                setDestination('');
            }
        } catch (error) {
            console.error("Error when I try to show profile", error);
        }
    };

    const handleLocationChange = (event) => setCurrentLocation(event.target.value);
    const handleDestinationChange = (event) => setDestination(event.target.value);

    useEffect(() => {
        const fetchUserInfo = async () => {
            try {
                const userInfo = await getUserInfo(jwt, apiForCurrentUserInfo, userId);
                const user = userInfo.user;
                setUserInfo(user);
                setInitialUser(user);

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

    useEffect(() => {
        const fetchRideData = async () => {
            try {
                const data = await getCurrentRide(jwt, apiEndpointForCurrentDrive, userId);

                if (data.error && data.error.status === 400) {
                    setClockSimulation("You don't have any active trips; you haven't requested one!");
                    return;
                }

                if (data.trip) {
                    if (!data.trip.accepted) {
                        setClockSimulation("Your current ticket has not been accepted by any driver!");
                    } else if (data.trip.accepted && data.trip.secondsToDriverArrive > 0) {
                        setClockSimulation(`The driver will arrive for: ${data.trip.secondsToDriverArrive} seconds`);
                    } else if (data.trip.accepted && data.trip.secondsToEndTrip > 0) {
                        setClockSimulation(`The journey will end in: ${data.trip.secondsToEndTrip} seconds`);
                    } else if (data.trip.accepted && data.trip.secondsToDriverArrive === 0 && data.trip.secondsToEndTrip === 0) {
                        setClockSimulation("Your trip has ended");
                    }
                } else {
                    setClockSimulation("You don't have any active trips; you haven't requested one!");
                }
            } catch (error) {
                console.error("Error fetching ride data:", error);
                setClockSimulation("An error occurred while fetching the trip data.");
            }
        };

        fetchRideData();
        const intervalId = setInterval(fetchRideData, 1000);
        return () => clearInterval(intervalId);
    }, [jwt, apiEndpointForCurrentDrive, userId]);

    // Definisanje poruka za prikazivanje overlay-a
    const isOverlayVisible = clockSimulation?.startsWith('The driver will arrive for') ||
        clockSimulation?.startsWith('The journey will end in') ||
        clockSimulation?.startsWith('Your trip has ended');

    return (
        <div style={{ display: 'flex', flexDirection: 'column', height: '100vh' }}>
            {isOverlayVisible && (
                <div className="overlay">
                    <p className="trip-status">{clockSimulation}</p>
                </div>
            )}
            <div className={`navbar ${isOverlayVisible ? 'blurred' : ''}`}>
                <div className="nav-left">
                    <span className="username-text">{username}:</span>
                    {clockSimulation && !isOverlayVisible && (
                        <p className="status-message">{clockSimulation}</p>
                    )}
                </div>
                <div className="nav-center">
                    <button className="nav-button" onClick={handleEditProfile}>Profile</button>
                    <button className="nav-button" onClick={handleNewDriveClick}>New drive</button>
                    <button className="nav-button" onClick={handleDriveHistory}>Driving history</button>
                    <button className="nav-button" onClick={handleRateTrips}>Rate rides</button>
                </div>
                <div className="nav-right">
                    <button className="button-logout" onClick={handleSignOut}>
                        <span>Sign out</span>
                    </button>
                </div>
            </div>
            <div style={{ display: 'flex', flexDirection: 'row', flex: 1, justifyContent: 'center' }}>
                <div className={`edit-profile-container ${isOverlayVisible ? 'blurred' : ''}`}>
                    {view === "editProfile" && (
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
                    {view === 'newDrive' && (
                        <div className="centered">
                            <div className="new-drive-container">
                                <h2>Book a New Drive</h2>
                                <div className="drive-form">
                                    <label>Current Location</label>
                                    <input
                                        type="text"
                                        value={currentLocation}
                                        onChange={handleLocationChange}
                                        placeholder="Enter your current location"
                                        className="input-field"
                                    />
                                    <label>Destination</label>
                                    <input
                                        type="text"
                                        value={destination}
                                        onChange={handleDestinationChange}
                                        placeholder="Enter your destination"
                                        className="input-field"
                                    />
                                    <button className="button-primary" onClick={handleEstimationSubmit}>
                                        Get Prediction
                                    </button>
                                    {estimation && (
                                        <>
                                            <p className="estimation-text">Predicted Ride Price: {estimation} €</p>
                                            <p className="estimation-text">Predicted Arrival Time of Blue Taxi: {driversArivalSeconds} min</p>
                                            <button className="button-primary" onClick={handleAcceptDriveSubmit}>
                                                Accept Drive
                                            </button>
                                        </>
                                    )}
                                </div>
                            </div>
                        </div>
                    )}
                    {view === 'driveHistory' && <RidesRider />}
                    {view === "rateTrips" && <Rate />}
                </div>
            </div>
        </div>
    );
}
