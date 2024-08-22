import axios from "axios";
import qs from 'qs';

//GetCurrentTrip
export async function getCurrentRide(jwt, apiEndpoint,userId) {     // Ova funkcija vraća podatke o trenutnoj vožnji korisnika i uključuje podatke kao što su vreme trajanja putovanja, vreme dolaska vozača itd.

    try {
        const config = {
            headers: {
                Authorization: `Bearer ${jwt}`,
                'Content-Type': 'application/json' //Oznaka da je sadržaj JSON format
            }
        };
        console.log(apiEndpoint);
        const queryParams = qs.stringify({ id: userId });         // Kreiramo query string parametre za slanje sa GET zahtevom. Koristimo `qs.stringify` da pretvorimo objekt u query string.

        const url = `${apiEndpoint}?${queryParams}`; // Kreiramo kompletan URL spajanjem API endpointa i query parametara
        const response = await axios.get(url, config);
        return response.data;
    } catch (error) {
        //console.error('Error fetching data (async/await):', error.message);
        //throw error;
        return { error: error.response };
    }
}

export async function getMyRides(jwt, apiEndpoint,userId) { //Dobijanje istorije voznji za odredjenog korisnika
    try {
        const config = {
            headers: {
                Authorization: `Bearer ${jwt}`,
                'Content-Type': 'application/json'
            }
        };
        console.log(apiEndpoint);
        const queryParams = qs.stringify({ id: userId });

        const url = `${apiEndpoint}?${queryParams}`;
        const response = await axios.get(url, config);
        return response.data;
    } catch (error) {
        //console.error('Error fetching data (async/await):', error.message);
        //throw error;
        return { error: error.response };
    }
}
//salje GET zahtev da bi se dobila lista voznji koje korisnik jos nije ocenio
export async function getAllUnRatedTrips(jwt, apiEndpoint) {
    try {
        const config = {
            headers: {
                Authorization: `Bearer ${jwt}`,
                'Content-Type': 'application/json'
            }
        };
        console.log(apiEndpoint);

        const response = await axios.get(apiEndpoint, config);
        return response.data;
    } catch (error) {
        return { error: error.response };
    }
}

export async function SubmitRating(apiEndpoint,jwt,rating,tripId) {//za odredjenu voznju postavi ocenu
    try {
        
        const response = await axios.put(apiEndpoint, {
            tripId: tripId,
            rating: rating
        }, {
            headers: {
                Authorization: `Bearer ${jwt}`
            }
        });
        console.log("This is response",response);
        return response.data;
    } catch (error) {
        console.error('Error while calling api for login user:', error);
        return error;
    }
}


