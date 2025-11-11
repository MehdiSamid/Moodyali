const API_BASE_URL = window.location.origin; // Should work for local and deployed

// --- Utility Functions ---

function setToken(token) {
    localStorage.setItem('jwtToken', token);
}

function getToken() {
    return localStorage.getItem('jwtToken');
}

function removeToken() {
    localStorage.removeItem('jwtToken');
}

function setUsername(username) {
    localStorage.setItem('username', username);
}

function getUsername() {
    return localStorage.getItem('username');
}

function displayMessage(elementId, message, isError = false) {
    const element = document.getElementById(elementId);
    element.textContent = message;
    element.style.color = isError ? 'red' : 'green';
    setTimeout(() => {
        element.textContent = '';
        element.style.color = '';
    }, 5000);
}

async function apiFetch(endpoint, options = {}) {
    const token = getToken();
    const headers = {
        'Content-Type': 'application/json',
        ...options.headers
    };

    if (token) {
        headers['Authorization'] = `Bearer ${token}`;
    }

    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
        ...options,
        headers
    });

    if (response.status === 401) {
        // Unauthorized - token expired or invalid
        logout();
        throw new Error('Unauthorized. Please log in again.');
    }

    if (!response.ok) {
        const errorText = await response.text();
        throw new Error(errorText || `API call failed with status ${response.status}`);
    }

    // Handle 204 No Content
    if (response.status === 204 || response.headers.get('Content-Length') === '0') {
        return null;
    }

    return response.json();
}

// --- Auth Handlers ---

function showDashboard() {
    document.getElementById('auth-section').style.display = 'none';
    document.getElementById('dashboard-section').style.display = 'block';
    document.getElementById('logout-btn').style.display = 'block';
    document.getElementById('dashboard-username').textContent = getUsername();
    loadDashboardData();
}

function showAuth() {
    document.getElementById('auth-section').style.display = 'block';
    document.getElementById('dashboard-section').style.display = 'none';
    document.getElementById('logout-btn').style.display = 'none';
    // Reset to login form
    document.getElementById('login-form-container').style.display = 'block';
    document.getElementById('register-form-container').style.display = 'none';
    document.getElementById('forgot-password-container').style.display = 'none';
}

function logout() {
    removeToken();
    setUsername('');
    showAuth();
}

async function handleLogin(event) {
    event.preventDefault();
    const username = document.getElementById('login-username').value;
    const password = document.getElementById('login-password').value;

    try {
        const data = await apiFetch('/auth/login', {
            method: 'POST',
            body: JSON.stringify({ username, password })
        });
        
        setToken(data.token);
        setUsername(data.username);
        displayMessage('login-message', 'Login successful!', false);
        showDashboard();
    } catch (error) {
        console.error('Login failed:', error);
        displayMessage('login-message', 'Login failed: Invalid credentials.', true);
    }
}

async function handleRegister(event) {
    event.preventDefault();
    const username = document.getElementById('register-username').value;
    const email = document.getElementById('register-email').value;
    const password = document.getElementById('register-password').value;

    try {
        await apiFetch('/auth/register', {
            method: 'POST',
            body: JSON.stringify({ username, email, password })
        });
        
        displayMessage('register-message', 'Registration successful! Please log in.', false);
        // Switch to login form
        document.getElementById('login-form-container').style.display = 'block';
        document.getElementById('register-form-container').style.display = 'none';
    } catch (error) {
        console.error('Registration failed:', error);
        displayMessage('register-message', error.message.includes('exists') ? 'Username or email already exists.' : 'Registration failed.', true);
    }
}

// --- Forgot Password Handler (Simple Mock) ---

async function handleForgotPassword(event) {
    event.preventDefault();
    const email = document.getElementById('forgot-email').value;
    const messageElement = document.getElementById('forgot-message');

    // In a real application, this would call an API endpoint.
    // For this simple implementation, we'll mock the success response.
    // We assume the backend will handle the actual email sending/token generation.
    
    try {
        // Mock API call for simplicity, assuming a new endpoint /auth/forgot-password
        await apiFetch('/auth/forgot-password', {
            method: 'POST',
            body: JSON.stringify({ email })
        });

        displayMessage('forgot-message', `If an account with ${email} exists, a password reset link has been sent.`, false);
        document.getElementById('forgot-password-form').reset();
    } catch (error) {
        console.error('Forgot password failed:', error);
        displayMessage('forgot-message', 'Failed to process request. Please try again.', true);
    }
}

// --- Mood Handlers ---

async function handleLogMood(event) {
    event.preventDefault();
    const emoji = document.getElementById('mood-select').value;
    const note = document.getElementById('mood-note').value;

    if (!emoji) {
        displayMessage('mood-log-message', 'Please select a mood.', true);
        return;
    }

    try {
        const data = await apiFetch('/mood', {
            method: 'POST',
            body: JSON.stringify({ emoji, note })
        });
        
        displayMessage('mood-log-message', 'Mood logged successfully!', false);
        // Clear input fields
        document.getElementById('mood-select').value = '';
        document.getElementById('mood-note').value = '';
        // Reload dashboard data to reflect change
        loadDashboardData();
    } catch (error) {
        console.error('Log mood failed:', error);
        displayMessage('mood-log-message', 'Failed to log mood.', true);
    }
}

async function loadTodayMood() {
    try {
        const mood = await apiFetch('/mood/today');
        document.getElementById('today-mood-emoji').textContent = mood.emoji;
    } catch (error) {
        if (error.message.includes('404')) {
            document.getElementById('today-mood-emoji').textContent = 'Not logged yet.';
        } else {
            console.error('Failed to load today mood:', error);
            document.getElementById('today-mood-emoji').textContent = 'Error loading mood.';
        }
    }
}

let weeklyMoodData = []; // Store data globally for chart use

async function loadWeeklyMoods() {
    try {
        const moods = await apiFetch('/mood/week');
        weeklyMoodData = moods; // Store data
        
        const list = document.getElementById('weekly-mood-list');
        list.innerHTML = ''; // Clear previous list

        moods.forEach(mood => {
            const li = document.createElement('li');
            const date = new Date(mood.date).toLocaleDateString('en-US', { weekday: 'short', month: 'short', day: 'numeric' });
            li.innerHTML = `
                <span>${date}</span>
                <span class="mood-emoji">${mood.emoji}</span>
            `;
            list.appendChild(li);
        });
    } catch (error) {
        console.error('Failed to load weekly moods:', error);
        document.getElementById('weekly-mood-list').innerHTML = '<li>Failed to load weekly data.</li>';
    }
}

let moodChartInstance = null;

function loadWeeklyMoodChart() {
    const ctx = document.getElementById('mood-chart').getContext('2d');

    if (moodChartInstance) {
        moodChartInstance.destroy();
    }

    // Sort data by date ascending for the chart
    const sortedData = [...weeklyMoodData].sort((a, b) => new Date(a.date) - new Date(b.date));

    const labels = sortedData.map(mood => new Date(mood.date).toLocaleDateString('en-US', { weekday: 'short' }));
    const scores = sortedData.map(mood => mood.Score);

    moodChartInstance = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: 'Mood Score',
                data: scores,
                borderColor: '#5c67f2',
                backgroundColor: 'rgba(92, 103, 242, 0.2)',
                tension: 0.3,
                fill: true
            }]
        },
        options: {
            responsive: true,
            scales: {
                y: {
                    beginAtZero: true,
                    max: 10,
                    ticks: {
                        stepSize: 1
                    }
                }
            },
            plugins: {
                legend: {
                    display: false
                },
                title: {
                    display: true,
                    text: 'Mood Score Trend Over 7 Days'
                }
            }
        }
    });
}

async function loadMoodStats() {
    try {
        const stats = await apiFetch('/mood/stats');
        document.getElementById('avg-score').textContent = stats.averageScore;
        document.getElementById('happy-days').textContent = stats.happyDays;
        document.getElementById('sad-days').textContent = stats.sadDays;
    } catch (error) {
        console.error('Failed to load mood stats:', error);
        document.getElementById('avg-score').textContent = 'N/A';
        document.getElementById('happy-days').textContent = 'N/A';
        document.getElementById('sad-days').textContent = 'N/A';
    }
}

function loadDashboardData() {
    loadTodayMood();
    loadWeeklyMoods();
    loadMoodStats();
    loadWeeklyMoodChart();
    loadRecommendation();
}

async function loadRecommendation() {
    const outputElement = document.getElementById('recommendation-output');
    const messageElement = document.getElementById('recommendation-message');

    outputElement.innerHTML = 'Generating recommendation...';
    messageElement.textContent = '';

    try {
        const data = await apiFetch('/recommendation', {
            method: 'GET'
        });

        outputElement.innerHTML = data.recommendation.replace(/\n/g, '<br>');
        messageElement.textContent = 'Recommendation generated successfully!';
        messageElement.style.color = 'green';
    } catch (error) {
        console.error('Recommendation failed:', error);
        outputElement.innerHTML = 'Failed to get recommendation.';
        messageElement.textContent = error.message || 'An error occurred.';
        messageElement.style.color = 'red';
    }
}

// --- Initialization ---

document.addEventListener('DOMContentLoaded', () => {
    // Check for existing token
    if (getToken()) {
        showDashboard();
    } else {
        showAuth();
    }

    // Event Listeners
    document.getElementById('login-form').addEventListener('submit', handleLogin);
    document.getElementById('register-form').addEventListener('submit', handleRegister);
    document.getElementById('mood-log-form').addEventListener('submit', handleLogMood);
    document.getElementById('logout-btn').addEventListener('click', logout);
    document.getElementById('forgot-password-form').addEventListener('submit', handleForgotPassword);

    // Form switching
    document.getElementById('show-register').addEventListener('click', (e) => {
        e.preventDefault();
        document.getElementById('login-form-container').style.display = 'none';
        document.getElementById('forgot-password-container').style.display = 'none';
        document.getElementById('register-form-container').style.display = 'block';
    });

    document.getElementById('show-login').addEventListener('click', (e) => {
        e.preventDefault();
        document.getElementById('login-form-container').style.display = 'block';
        document.getElementById('register-form-container').style.display = 'none';
        document.getElementById('forgot-password-container').style.display = 'none';
    });

    document.getElementById('show-forgot-password').addEventListener('click', (e) => {
        e.preventDefault();
        document.getElementById('login-form-container').style.display = 'none';
        document.getElementById('register-form-container').style.display = 'none';
        document.getElementById('forgot-password-container').style.display = 'block';
    });

    document.getElementById('show-login-from-forgot').addEventListener('click', (e) => {
        e.preventDefault();
        document.getElementById('login-form-container').style.display = 'block';
        document.getElementById('register-form-container').style.display = 'none';
        document.getElementById('forgot-password-container').style.display = 'none';
    });
});
