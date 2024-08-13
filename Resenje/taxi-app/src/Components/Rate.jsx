import React, { useState, useEffect } from 'react';
import '../Styles/DriversViewPage.css'; // Import your CSS file for styling
import { getAllUnRatedTrips } from '../Services/RiderServices';
import { FaStar } from 'react-icons/fa';
import { SubmitRating } from '../Services/RiderServices';
export default function Rate() {
    const [rides, setRides] = useState([]);
    const [selectedTripId, setSelectedTripId] = useState(null);
    const [selectedRating, setSelectedRating] = useState(0);
    const token = localStorage.getItem('token');
    const apiEndpoint = process.env.REACT_APP_GET_ALL_UNRATED_TRIPS;
    const ratintEndpoint = process.env.REACT_APP_SUMBIT_RATING;
    // Function to fetch all unrated trips
    const fetchDrivers = async () => {
        try {
            const data = await getAllUnRatedTrips(token, apiEndpoint);
            console.log("Rides:", data.rides);
            setRides(data.rides);
        } catch (error) {
            console.error('Error fetching drivers:', error);
        }
    };

    useEffect(() => {
        fetchDrivers();
    }, []);

    // Function to handle rating selection
    const handleRating = (tripId, rating) => {
        setSelectedTripId(tripId);
        setSelectedRating(rating);
    };

    // Function to submit rating to the backend
    const submitRatingToBackend = async () => {
        if (selectedTripId && selectedRating) {
            try {
                console.log("Usao");
                console.log(selectedTripId);
                console.log(selectedRating);
                //await submitRating(token, selectedTripId, selectedRating);
                const data = await SubmitRating(ratintEndpoint,token,selectedRating,selectedTripId);
                console.log(data);
                console.log("Rating submitted successfully");
                fetchDrivers(); // Refresh the list after submitting
            } catch (error) {
                console.error('Error submitting rating:', error);
            }
        }
    };

    return (
        <div className="centered" style={{ width: '100%', height: '10%' }}>
            <table className="styled-table">
                <thead>
                    <tr>
                        <th>From</th>
                        <th>To</th>
                        <th>Price</th>
                        <th>Rating</th>
                        <th>Action</th>
                    </tr>
                </thead>
                <tbody>
                    {rides.map((ride, index) => (
                        <tr key={index}>
                            <td>{ride.currentLocation}</td>
                            <td>{ride.destination}</td>
                            <td>{ride.price}</td>
                            <td style={{ textAlign: 'center' }}>
                                <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center' }}>
                                    {[...Array(5)].map((_, i) => (
                                        <FaStar
                                            key={i}
                                            size={20}
                                            color={i < (selectedTripId === ride.tripId ? selectedRating : 0) ? 'gold' : 'grey'}
                                            onClick={() => handleRating(ride.tripId, i + 1)}
                                            style={{ cursor: 'pointer' }}
                                        />
                                    ))}
                                </div>
                            </td>
                            <td>
                                {selectedTripId === ride.tripId && (
                                    <button
                                        onClick={submitRatingToBackend}
                                        style={{
                                            borderRadius: '20px',
                                            padding: '5px 10px',
                                            color: 'white',
                                            fontWeight: 'bold',
                                            cursor: 'pointer',
                                            outline: 'none',
                                            background: 'green'
                                        }}
                                    >
                                        Submit
                                    </button>
                                )}
                            </td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
}