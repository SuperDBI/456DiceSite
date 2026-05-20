-- PostgreSQL Database Schema for 456Dice User Management

-- Users Table
CREATE TABLE users (
    user_id SERIAL PRIMARY KEY,
    username VARCHAR(50) NOT NULL,
    email VARCHAR(100) NOT NULL UNIQUE,
    password_hash TEXT NOT NULL,
    profile_picture TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Friends Table
CREATE TABLE friends (
    user_id INT REFERENCES users(user_id),
    friend_id INT REFERENCES users(user_id),
    PRIMARY KEY (user_id, friend_id)
);

-- Game History Table
CREATE TABLE game_history (
    game_id SERIAL PRIMARY KEY,
    user_id INT REFERENCES users(user_id),
    game_date TIMESTAMP NOT NULL,
    score INT NOT NULL
);

-- Account Information Table
CREATE TABLE account_info (
    user_id INT PRIMARY KEY REFERENCES users(user_id),
    player_id VARCHAR(50) NOT NULL,
    payment_info TEXT,
    device_id TEXT
);

-- Rewards History Table
CREATE TABLE rewards_history (
    reward_id SERIAL PRIMARY KEY,
    user_id INT REFERENCES users(user_id),
    reward_name VARCHAR(100),
    reward_date TIMESTAMP NOT NULL
);

-- Purchases Table
CREATE TABLE purchases (
    purchase_id SERIAL PRIMARY KEY,
    user_id INT REFERENCES users(user_id),
    item_name VARCHAR(100),
    purchase_date TIMESTAMP NOT NULL
);

-- Shop Items Table
CREATE TABLE shop_items (
    item_id SERIAL PRIMARY KEY,
    item_name VARCHAR(100),
    price DECIMAL(10, 2) NOT NULL
);

-- Shopping Cart Table
CREATE TABLE shopping_cart (
    cart_id SERIAL PRIMARY KEY,
    user_id INT REFERENCES users(user_id),
    item_id INT REFERENCES shop_items(item_id),
    quantity INT NOT NULL
);