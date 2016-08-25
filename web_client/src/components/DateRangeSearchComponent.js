import React from 'react';



export default class DateRangeSearchComponent extends React.Component{
  
  constructor(props){
		super(props);
		this.state = {
		}
	}

  _handleOnSubmit(evt){
		evt.preventDefault();

		let queryPath = []
		for(let p in this.state){
			if (this.state[p].length > 0)
			  queryPath.push(`${p}=${this.state[p]}`);
		}

		let url = this.props.href;
		url += (queryPath.length === 0) ? "" : '?'+queryPath.join('&');
    console.log(url)
		this.props.onSearch(url);
	}

  render(){
    let afterLabelName = "after";
    let beforeLabelName = "before";
    return (
      <div className="text-right">
        <form onSubmit={this._handleOnSubmit.bind(this)}
              className="form-inline" role="form">
          <div className="form-group">
            <label for={afterLabelName}>After:</label>
            <input
              type="date"
              id={afterLabelName}
              className="form-control"
              onChange={ evt => this.setState({[afterLabelName]: evt.target.value}) }
              name={afterLabelName}/>
          </div>
          <div className="form-group">
            <label for={beforeLabelName}>Before:</label>
            <input
              type="date"
              name={beforeLabelName}
              className="form-control"
              onChange={ evt => this.setState({[beforeLabelName]: evt.target.value}) }
              id={beforeLabelName}/>
          </div>
          <button type="submit" className="btn btn-success">Search</button>
        </form>
      </div>
    );
  }

}