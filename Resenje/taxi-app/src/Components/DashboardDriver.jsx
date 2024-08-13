import React, { useState, useEffect } from 'react';
import { useNavigate } from "react-router-dom";
import '../Styles/DashboardPage.css';
import '../Styles/ProfilePage.css';
import { MdPerson } from 'react-icons/md';
import { FaCar, FaRoad, FaSignOutAlt } from 'react-icons/fa';
import { makeImage, convertDateTimeToDateOnly, changeUserFields } from '../Services/ProfileServices.js';
import { getUserInfo } from '../Services/ProfileServices.js';
import { getAllAvailableRides, AcceptDrive, getCurrentRide } from '../Services/DriverServices.js';
import RidesDriver from './RidesDriver.jsx';

export default function DashboardDriver(props) {
    const user = props.user;
    const apiEndpoint = process.env.REACT_APP_CHANGE_USER_FIELDS;
    const userId = user.id;
    localStorage.setItem("userId", userId);
    const jwt = localStorage.getItem('token');
    const navigate = useNavigate();

    const apiForCurrentUserInfo = process.env.REACT_APP_GET_USER_INFO;
    const apiEndpointForCurrentRide = process.env.REACT_APP_CURRENT_TRIP_DRIVER;
    const [currentUser, setUserInfo] = useState('');

    const [address, setAddress] = useState('');
    const [averageRating, setAverageRating] = useState('');
    const [birthday, setBirthday] = useState('');
    const [email, setEmail] = useState('');
    const [firstName, setFirstName] = useState('');
    const [imageFile, setImageFile] = useState('');
    const [isBlocked, setIsBlocked] = useState(false);
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
    const [tripIsActive, setTripIsActive] = useState(false);
    const [rides, setRides] = useState([]);
    const [currentRide, setCurrentRides] = useState();
    const apiToGetAllRides = process.env.REACT_APP_GET_ALL_RIDES_ENDPOINT;
    const [clockSimulation, setClockSimulation] = useState('');
    const [showOverlay, setShowOverlay] = useState(false);
    const [message, setMessage] = useState('You don\'t have an active trip!');

    // Funkcija za ažuriranje poruke
    const updateMessage = () => {
        if (rides.length === 0) {
            setMessage('You don\'t have an active trip!');
        } else {
            setMessage('Available rides, please accept one.');
        }
    };

    // UseEffect za periodičnu promenu poruke
    useEffect(() => {
        updateMessage(); // Pozivamo funkciju odmah kako bismo postavili početnu poruku
        const intervalId = setInterval(updateMessage, 5000); // Ažuriramo poruku svakih 5 sekundi

        return () => clearInterval(intervalId); // Čišćenje intervala na unmount
    }, [rides]);

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

    const handleSignOut = () => {
        localStorage.removeItem('token');
        navigate('/');
    };

    const handleEditProfile = async () => setView('editProfile');

    const handleViewRides = async () => {
        try {
            await fetchRides();
            setView('rides');
        } catch (error) {
            console.error("Error when I try to show profile", error);
        }
    };

    const handleDriveHistory = () => setView("driveHistory");

    const fetchRides = async () => {
        try {
            const data = await getAllAvailableRides(jwt, apiToGetAllRides);
            setRides(data.rides);
        } catch (error) {
            console.error('Error fetching drivers:', error);
        }
    };

    const handleAcceptNewDrive = async (tripId) => {
        try {
            const data = await AcceptDrive(process.env.REACT_APP_ACCEPT_RIDE, userId, tripId, jwt);
            setCurrentRides(data.ride);
            setTripIsActive(true);
            setShowOverlay(true); // Show overlay when the trip is active
            // Pozivamo fetchRides da ažuriramo tabelu
            fetchRides();
        } catch (error) {
            console.error('Error accepting drive:', error.message);
        }
    };

    const handleEditClick = () => setIsEditing(true);

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

    const handleImageChange = (e) => {
        const file = e.target.files[0];
        setSelectedFile(file);
        const reader = new FileReader();
        reader.onloadend = () => {
            setImageFile(reader.result);
        };
        if (file) {
            reader.readAsDataURL(file);
        }
    };

    useEffect(() => {
        const fetchRideData = async () => {
            try {
                const data = await getCurrentRide(jwt, apiEndpointForCurrentRide, userId);

                if (data.error && data.error.status === 400) {
                    setClockSimulation("You don't have an active trip!");
                    setShowOverlay(false);
                    setTripIsActive(false);
                    fetchRides();
                    return;
                }

                if (data.trip) {
                    if (data.trip.accepted && data.trip.secondsToDriverArrive > 0) {
                        setClockSimulation(`You will arrive in: ${data.trip.secondsToDriverArrive} seconds`);
                    } else if (data.trip.accepted && data.trip.secondsToEndTrip > 0) {
                        setClockSimulation(`The trip will end in: ${data.trip.secondsToEndTrip} seconds`);
                    } else if (data.trip.accepted && data.trip.secondsToDriverArrive === 0 && data.trip.secondsToEndTrip === 0) {
                        setClockSimulation("Your trip has ended");
                        setShowOverlay(false); // Hide overlay when trip ends
                        setTripIsActive(false);
                        fetchRides(); // Ažuriranje tabele vožnji
                    }
                } else {
                    setClockSimulation("You don't have an active trip!");
                    setShowOverlay(false);
                    setTripIsActive(false);
                    fetchRides();
                }
            } catch (error) {
                setClockSimulation("An error occurred while fetching the trip data.");
                setShowOverlay(false);
                setTripIsActive(false);
                fetchRides();
            }
        };

        fetchRideData();
        const intervalId = setInterval(fetchRideData, 1000);

        return () => clearInterval(intervalId);
    }, [jwt, apiEndpointForCurrentRide, userId]);

    // Proveravamo da li treba prikazati overlay
    const isOverlayVisible = clockSimulation?.startsWith('You will arrive in') ||
        clockSimulation?.startsWith('The trip will end in') ||
        clockSimulation?.startsWith('Your trip has ended');

    return (
        <div style={{ display: 'flex', flexDirection: 'column', height: '100vh' }}>
            {showOverlay && (
                <div className="overlay">
                    <p className="trip-status">{clockSimulation}</p>
                </div>
            )}
            <div className="navbar">
                <div className="nav-left">
                    <span className="username-text">{username}:</span>
                    {message && !isOverlayVisible && (
                        <p className="status-message">{message}</p>
                    )}
                </div>
                <div className="nav-center">
                    {!tripIsActive && (
                        <>
                            <button className="nav-button" onClick={handleEditProfile}>Profile</button>
                            <button className="nav-button" onClick={handleViewRides}>New rides</button>
                            <button className="nav-button" onClick={handleDriveHistory}>Rides history</button>
                        </>
                    )}
                </div>
                <div className="nav-right">
                    <button className="button-logout" onClick={handleSignOut}>
                        <span>Sign out</span>
                    </button>
                </div>
            </div>
            <div style={{ display: 'flex', flexDirection: 'row', flex: 1, justifyContent: 'center', backgroundImage: 'url("/public/Images/Pozadina.jpg")' }}>
                <div className="edit-profile-container">
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
                    {view === 'rides' && (
                        <div className="centered" style={{ width: '100%', height: '10%' }}>
                            <table className="styled-table" style={{ width: '70%' }}>
                                <thead>
                                    <tr>
                                        <th>Location</th>
                                        <th>Destination</th>
                                        <th>Price</th>
                                        <th>Confirmation</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {rides.map((val) => (
                                        <tr key={val.tripId}>
                                            <td>{val.currentLocation}</td>
                                            <td>{val.destination}</td>
                                            <td>{val.price}</td>
                                            <td>
                                                <button
                                                    style={{
                                                        borderRadius: '20px',
                                                        padding: '5px 10px',
                                                        color: 'white',
                                                        fontWeight: 'bold',
                                                        cursor: 'pointer',
                                                        outline: 'none',
                                                        background: 'green'
                                                    }}
                                                    onClick={() => handleAcceptNewDrive(val.tripId)}
                                                >
                                                    Accept
                                                </button>
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </table>
                        </div>
                    )}
                    {view === 'driveHistory' && <RidesDriver />}
                </div>
            </div>
        </div>
    );
}
