var path = require('path');
var webpack = require('webpack');
var CopyWebpackPlugin = require('copy-webpack-plugin');

var dirJs = path.resolve(__dirname, 'src');
var dirBuild = '..\\issues_web_api\\issues_web_api\\client';
var dirHtml = path.resolve(__dirname, 'html');

module.exports = {
  entry: path.resolve(dirJs, 'main.js'),
  output: {
    path: dirBuild,
    filename: 'bundle.js'
  }
  ,module: {    
    loaders: [
      {
        loader: 'babel-loader',
        test: dirJs,
      },
      {
        loader: "style-loader!css-loader",
        test: /\.css$/
      }
    ]
  }  
  ,plugins: [      
      new CopyWebpackPlugin([
        { from: dirHtml }
      ])      
  ]
  ,devServer: {
    port: 8000,
    outputPath: dirBuild,
    contentBase: dirBuild,
  },
}