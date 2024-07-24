const Dotenv = require('dotenv-webpack');
const webpack = require('webpack');

module.exports = {
  plugins: [
    new Dotenv({
      systemvars: true, // Load all system environment variables
    }),
    new webpack.ProvidePlugin({
      process: 'process/browser',
    }),
  ],
};
