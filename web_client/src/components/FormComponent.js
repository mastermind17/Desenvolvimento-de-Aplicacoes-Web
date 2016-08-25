import React from 'react';

/**
* Este componente representa um formulário. É criado com base num
* template (collection mediatype property) fornecido via 'props'.
*/
export default class FormComponent extends React.Component{

    constructor(props){
        super(props);
        this.state={}
    }

    handleFormSubmit(evt){
        evt.preventDefault();

        let template = {
          template: {}
        };
        template.template = JSON.parse(JSON.stringify(this.props.template));
        console.log(template.template)
        //prencher objecto com novos valores
        template.template.data.forEach(obj => {
          obj.value = obj.value ? obj.value : this.state[obj.name];
          delete obj.prompt;
        });
        this.props.onNewItemHandler(this.props.targetSubmitHref, template);
        evt.target.reset(); //clean fields
    }


    render() {
        if(!this.props.template.data){
            return null;
        }
        return (
        	<div>
                <h3>Create new {this.props.class}:</h3>

                <form onSubmit={this.handleFormSubmit.bind(this)}>

                    {this.props.template.data.map((obj, idx) => {
                      return (<div key={idx} className={obj.value ? "hidden form-group" : "form-group"}>
                                  <label for={obj.name}>{obj.name}</label>
                                  <input ref={obj.name}
                                      type="text"
                                      className="form-control"
                                      value={obj.value ? obj.value : undefined }
                                      id={obj.name}
                                      placeholder={obj.prompt}
                                      name={obj.name}
                                      onChange={evt => {
                                          this.setState({[obj.name]: evt.target.value});
                                      }}
                                  />
                              </div>)
                      })
                    }

                    <button type="submit" className="btn btn-success">
                        Submit
                    </button>
                </form>
		    </div>
        	);
    }
}

FormComponent.propTypes = {
    targetSubmitHref: React.PropTypes.string.isRequired,
    onNewItemHandler: React.PropTypes.func.isRequired,
    template: React.PropTypes.object.isRequired
}
